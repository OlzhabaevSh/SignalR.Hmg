using Signalr.Hmg.Core;
using Signalr.Hmg.Core.Models;

namespace Signalr.Hmg.Tests.E2es.E2eTests
{
    public class Tests
    {
        private const string csProjPath = @"C:\Users\solzhabayev\source\repos\SignalR.Hmg\tests\e2es\Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice\Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.csproj";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task If_Parsing_Works()
        {
            var service = SignalrMetadataService.CreateMetadataGenerator(csProjPath)
                .ParseAll();

            var result = await service.GenerateMetadataAsync();

            // result
            var expectedResult = GenerateCorrectVersionOfSignalRMetadata();

            var methods = result.Methods;

            var events = result.Events;

            var entityes = result.Entities;

        }

        private SignalrMetadata GenerateCorrectVersionOfSignalRMetadata() 
        {
            return new SignalrMetadata()
            {
                Methods = new []
                {
                    new HubMethod()
                    {
                        HubName = "ChatHub",
                        Name = "SendMessageAsync",
                        Arguments = new HubMethodArgument[]
                        {
                            new HubMethodArgument()
                            {
                                OrderNumber = 0,
                                Name = "from",
                                TypeName = "String"
                            },
                            new HubMethodArgument()
                            {
                                OrderNumber = 1,
                                Name = "message",
                                TypeName = "String"
                            }
                        }
                    },
                    new HubMethod()
                    {
                        HubName = "ChatHub",
                        Name = "LoginUser",
                        Arguments = new HubMethodArgument[]
                        {
                            new HubMethodArgument()
                            {
                                OrderNumber = 0,
                                Name = "nickname",
                                TypeName = "String"
                            },
                            new HubMethodArgument()
                            {
                                OrderNumber = 1,
                                Name = "age",
                                TypeName = "Int32"
                            }
                        }
                    },
                    new HubMethod()
                    {
                        HubName = "UserNotificationHub",
                        Name = "SendTestNotification",
                        Arguments = new HubMethodArgument[]
                        {
                            new HubMethodArgument()
                            {
                                OrderNumber = 0,
                                Name = "author",
                                TypeName = "String"
                            }
                        }
                    }
                },
                Events = new[] 
                {
                    new HubEvent() 
                    {
                        HubName= "UserNotificationHub",
                        Name = "notifiesTriggered",
                        Arguments = new [] 
                        {
                            new HubEventArgument() 
                            {
                                OrderNumber = 0,
                                TypeName = "UserNotificationHubNotifiesTriggeredModel"
                            }
                        }
                    }
                },
                Entities = new[] 
                {
                    new Entity() 
                    {
                        Name = "String",
                        Properties = Array.Empty<EntityProperty>()
                    },
                    new Entity()
                    {
                        Name = "Int32",
                        Properties = Array.Empty<EntityProperty>()
                    },
                    new Entity()
                    {
                        Name = "UserNotificationHubNotifiesTriggeredModel",
                        Properties = new [] 
                        {
                            new EntityProperty() 
                            {
                                OrderNumber= 0,
                                Name = "RandomValue",
                                TypeName = "int"
                            },
                            new EntityProperty()
                            {
                                OrderNumber= 0,
                                Name = "CurrentDate",
                                TypeName = "DateTime"
                            }
                        }
                    },
                } 
            };
        }
    }
}