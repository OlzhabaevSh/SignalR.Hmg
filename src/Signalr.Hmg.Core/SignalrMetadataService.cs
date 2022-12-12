using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Signalr.Hmg.Core.Models;

namespace Signalr.Hmg.Core
{
    public class SignalrMetadataService
    {
        public static SignalrMetadataService CreateMetadataGenerator(string csprojPath) 
        {
            return new SignalrMetadataService(csprojPath);
        }

        private readonly string csprojPath;

        private bool parseMethods = false;
        private bool parseEvents = false;
        private bool parseEntities = false;

        private SignalrMetadataService(string csprojPath) 
        {
            this.csprojPath = csprojPath;
        }

        public SignalrMetadataService ParseMethods(bool parse) 
        {
            this.parseMethods= parse;
            return this;
        }

        public SignalrMetadataService ParseEvents(bool parse)
        {
            this.parseEvents = parse;
            return this;
        }

        public SignalrMetadataService ParseEntities(bool parse)
        {
            this.parseEntities = parse;
            return this;
        }

        public SignalrMetadataService ParseAll() 
        {
            return this
                .ParseMethods(true)
                .ParseEvents(true)
                .ParseEntities(true);
        }

        public async Task<SignalrMetadata> GenerateMetadataAsync()
        {
            MSBuildLocator.RegisterDefaults();
            var workspace = MSBuildWorkspace.Create();

            var csproject = await workspace.OpenProjectAsync(this.csprojPath)
                .ConfigureAwait(false);

            var compilation = await csproject.GetCompilationAsync()
                .ConfigureAwait(false);

            var hubMethods = this.ProcessMethods(compilation);
            var hubEvents = await this.ProcessEvents(csproject, compilation);
            
            var cache = this.MakeClassTree(compilation);

            var allEntityNames = GetAllEntities(hubMethods, hubEvents);

            var entities = ProcessEntities(compilation, allEntityNames, cache);

            return new SignalrMetadata
            {
                Entities = entities,
                Methods = hubMethods.SelectMany(x => x.Value).ToArray(),
                Events = hubEvents.SelectMany(x => x.Value).ToArray()
            };
        }

        public Dictionary<string, SyntaxTree> MakeClassTree(Compilation compilation)
        {
            var res = new Dictionary<string, SyntaxTree>();

            foreach (var tree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(tree);

                var root = tree.GetCompilationUnitRoot();

                foreach (var member in root.Members)
                {
                    var kind = member.Kind();

                    var namespaceDecalration = (NamespaceDeclarationSyntax)member;

                    var classDeclarations = namespaceDecalration.Members;

                    foreach (ClassDeclarationSyntax cls in classDeclarations)
                    {
                        var name = cls.Identifier.Text;

                        if (!res.ContainsKey(name))
                        {
                            res.Add(name, tree);
                        }
                    }
                }
            }

            return res;
        }

        private Dictionary<string, HubMethod[]> ProcessMethods(Compilation compilation)
        {
            if (!this.parseMethods) 
            {
                return new Dictionary<string, HubMethod[]>();
            }

            var result = new Dictionary<string, HubMethod[]>();

            var trees = compilation.SyntaxTrees;

            // TODO: shoud be changed
            // it will be better to find all classes who is extender of Hub
            // insteed of searching files who contains "hub" in the name.
            var hubTrees = trees
                .Where(x => x.FilePath.EndsWith("Hub.cs"))
                .ToList();

            // handle hubs
            foreach (var hubTree in hubTrees)
            {
                var semanticModel = compilation.GetSemanticModel(hubTree);

                var root = hubTree.GetCompilationUnitRoot();

                // class member
                var firstMember = root.Members.First();

                var firstMemberKind = firstMember.Kind();

                var hubDeclaration = (NamespaceDeclarationSyntax)firstMember;

                var clsDeclaration = (ClassDeclarationSyntax)hubDeclaration.Members[0];

                // query will return only:
                // 1. METHODS
                var methods = clsDeclaration.Members
                    .Select(x => x as MethodDeclarationSyntax)
                    .Where(x => x != null && x.Modifiers.Any(f => string.Equals(f.Text, "public", StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();

                var hubName = clsDeclaration.Identifier.Text;
                var events = new List<HubMethod>();
                foreach (var item in methods)
                {
                    var methodArgs = item.ParameterList.Parameters
                        .Select(x => x.Identifier)
                        .ToList();

                    var eventItem = new HubMethod()
                    {
                        Name = item.Identifier.Text,
                        Arguments = new List<HubMethodArgument>()
                    };

                    int i = 0;
                    foreach (var arg in methodArgs)
                    {
                        if (arg.Parent == null)
                        {
                            throw new NullReferenceException();
                        }

                        var fch = arg.Parent.ChildNodes().First();

                        var typeInfo = semanticModel.GetTypeInfo(fch);
                        var argType = typeInfo.Type;

                        eventItem.Arguments.Add(new HubMethodArgument()
                        {
                            OrderNumber = i,
                            Name = arg.ValueText,
                            TypeName = argType.Name
                        });

                        i++;
                    }

                    events.Add(eventItem);
                }

                result.Add(hubName, events.ToArray());
            }

            return result;
        }

        private Entity[] ProcessEntities(
            Compilation compilation, 
            string[] entityNames, 
            Dictionary<string, 
            SyntaxTree> cache) 
        {
            if (!this.parseEntities) 
            {
                return Array.Empty<Entity>();
            }

            var result = new List<Entity>();

            var entityHash = new HashSet<string>(entityNames);

            // go by entity tree
            foreach (var entityName in entityNames) 
            {
                if (!cache.ContainsKey(entityName))
                    continue;

                var tree = cache[entityName];

                var model = compilation.GetSemanticModel(tree);

                var root = tree.GetCompilationUnitRoot();

                var firstMember = root.Members.First();

                var firstMemberKind = firstMember.Kind();

                var hubDeclaration = (NamespaceDeclarationSyntax)firstMember;

                var currentClass = hubDeclaration.Members
                    .Select(x => x as ClassDeclarationSyntax)
                    .First(x =>
                        x != null
                        && string.Equals(
                            x.Identifier.Text,
                            entityName,
                            StringComparison.InvariantCultureIgnoreCase));

                var members = currentClass.Members
                    .Select(x => x as PropertyDeclarationSyntax)
                    .Where(x =>
                        x != null
                        && x.Modifiers.Any(f => string.Equals(
                            f.Text, "public", StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();

                foreach (var item in members) 
                {
                    var propTypeName = string.Empty;

                    if (item.Type is PredefinedTypeSyntax predTs)
                    {
                        propTypeName = predTs.Keyword.ValueText;
                    }
                    else if (item.Type is IdentifierNameSyntax idNs) 
                    {
                        propTypeName = idNs.Identifier.Text;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }

                    if (!entityHash.Contains(propTypeName))
                    {
                        entityHash.Add(propTypeName);
                    }
                }

            }

            foreach (var entityName in entityHash)
            {
                if (!cache.ContainsKey(entityName))
                {
                    result.Add(new Entity()
                    {
                        Name = entityName,
                        Properties = Array.Empty<EntityProperty>()
                    });

                    continue;
                }

                var tree = cache[entityName];

                var model = compilation.GetSemanticModel(tree);

                var root = tree.GetCompilationUnitRoot();

                var firstMember = root.Members.First();

                var firstMemberKind = firstMember.Kind();

                var hubDeclaration = (NamespaceDeclarationSyntax)firstMember;

                var currentClass = hubDeclaration.Members
                    .Select(x => x as ClassDeclarationSyntax)
                    .First(x =>
                        x != null
                        && string.Equals(
                            x.Identifier.Text,
                            entityName,
                            StringComparison.InvariantCultureIgnoreCase));

                var members = currentClass.Members
                    .Select(x => x as PropertyDeclarationSyntax)
                    .Where(x =>
                        x != null
                        && x.Modifiers.Any(f => string.Equals(
                            f.Text, "public", StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();

                var nEntity = new Entity()
                {
                    Name= entityName,
                    Properties = new List<EntityProperty>()
                };

                foreach (var item in members)
                {
                    var propName = item.Identifier.ValueText;

                    var propTypeName = string.Empty;

                    if (item.Type is PredefinedTypeSyntax prTs)
                    {
                        propTypeName = prTs.Keyword.ValueText;
                    }
                    else if (item.Type is IdentifierNameSyntax idNs)
                    {
                        propTypeName= idNs.Identifier.ValueText;
                    }
                    else 
                    {
                        throw new InvalidOperationException();
                    }

                    nEntity.Properties.Add(new EntityProperty() 
                    {
                        OrderNumber = 0,
                        Name = propName,
                        TypeName = propTypeName,
                    });
                }

                result.Add(nEntity);
            }

            return result.ToArray();
        }

        private async Task<Dictionary<string, HubEvent[]>> ProcessEvents(
            Project csproject, 
            Compilation compilation, 
            string methodName = "SendAsync") 
        {
            if (!this.parseEvents) 
            {
                return new Dictionary<string, HubEvent[]>();
            }

            var result = new Dictionary<string, List<HubEvent>>();

            var events = new List<(string hubName, string eventName, ICollection<string> argumentTypes)>();

            foreach(var document in csproject.Documents) 
            {
                var model = await document.GetSemanticModelAsync()
                    .ConfigureAwait(false);

                var methodInvocation = await document.GetSyntaxRootAsync();

                var allNodesWithThisMethod = methodInvocation.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(x =>
                        (x.Expression as MemberAccessExpressionSyntax) != null &&
                        ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == methodName)
                    .ToList();

                foreach (var item in allNodesWithThisMethod) 
                {
                    var methodSymbol = model.GetSymbolInfo(item).Symbol as IMethodSymbol;
                    var methodMetadataName = $"{methodSymbol.ContainingType.ContainingAssembly.Name}.{methodSymbol.ContainingType.Name}";

                    if (string.Equals(methodMetadataName, "Microsoft.AspNetCore.SignalR.Core.ClientProxyExtensions", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // handle event name
                        var eventArg = item.ArgumentList.Arguments.First();

                        var eventName = eventArg.Expression.ChildTokens().First().ValueText;

                        // handle args
                        var arguments = item.ArgumentList.Arguments
                            .Skip(1)
                            .ToList();

                        var argumentResult = new List<string>();

                        foreach (var argument in arguments)
                        {
                            var argSymbol = model.GetSymbolInfo(argument.Expression).Symbol;
                            var argName = argSymbol.Name;

                            var argTypeInfo = model.GetTypeInfo(argument.Expression);
                            var typeName = argTypeInfo.Type;

                            argumentResult.Add(typeName.Name);
                        }

                        // handle hub name
                        // TODO:SendAsync("userPosted", id) wasn't founded
                        var nodes = item.DescendantNodes()
                            .OfType<MemberAccessExpressionSyntax>();

                        var hubName = string.Empty;
                        foreach (var parentNode in nodes)
                        {
                            var parentNodeInfo = model.GetTypeInfo(parentNode.Expression);
                            var parentNodeType = parentNodeInfo.Type;
                            var parentNodeTypeName = parentNodeType.Name;

                            if (string.Equals(parentNodeTypeName, "IHubContext", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // called out side from Hub
                                // example: Namespace.ClassName<NameSpace.ClassName>
                                var metadataName = parentNodeInfo.Type.ToDisplayString();
                                var genericName = metadataName
                                    .Replace("<", ".")
                                    .Replace(">", "")
                                    .Split(".")
                                    .Last();

                                hubName = genericName;

                                break;
                            }

                            if (string.Equals(parentNodeType?.BaseType?.Name, "Hub", StringComparison.InvariantCultureIgnoreCase))
                            {
                                hubName = parentNodeTypeName;
                                break;
                            }
                        }

                        if (hubName != string.Empty)
                        {
                            events.Add((hubName, eventName, argumentResult));
                        }
                    }
                }
            }

            foreach (var item in events)
            {
                if (!result.ContainsKey(item.hubName))
                {
                    result.Add(item.hubName, new List<HubEvent>());
                }

                var hub = result[item.hubName];

                if (!hub.Any(x => string.Equals(x.Name, item.eventName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    hub.Add(new HubEvent()
                    {
                        Name = item.eventName,
                        Arguments = item.argumentTypes
                            .Select((x, i) => new HubEventArgument()
                            {
                                OrderNumber = i,
                                TypeName = x
                            }).ToList()
                    });
                }
            }

            return result.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        private string[] GetAllEntities(
            Dictionary<string, HubMethod[]> hubMethods, 
            Dictionary<string, HubEvent[]> hubEvents) 
        {
            var methodEntityNames = hubMethods
                .SelectMany(hub => hub.Value
                    .SelectMany(ev => ev.Arguments
                        .Select(arg => arg.TypeName)));

            var eventEntityNames = hubEvents
                .SelectMany(hub => hub.Value
                    .SelectMany(ev => ev.Arguments
                        .Select(arg => arg.TypeName)));

            var allEntityNames = methodEntityNames
                .Concat(eventEntityNames)
                .Distinct()
                .ToArray();

            return allEntityNames;
        }
    }
}
