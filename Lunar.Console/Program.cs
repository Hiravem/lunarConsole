using Lunar.Console.Infrastructure;
using Lunar.Console.Presentation;

var input = new InputReader();
var output = new OutputWriter();
var random = new RandomService();
var eventBus = new InMemoryEventBus();
var itemFactory = new Lunar.Core.Domain.Items.ItemFactory();
var saveRepository = new JsonSaveRepository();

var presenter = new ConsoleGamePresenter(input, output, random, eventBus, itemFactory, saveRepository);
presenter.Run();
