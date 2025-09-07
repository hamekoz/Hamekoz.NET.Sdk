using Hamekoz.Api.Controllers;
using Hamekoz.Api.Example.Models;
using Hamekoz.Api.Services;

namespace Hamekoz.Api.Example.Controllers;

public class PersonsController(ICrudService<Person> crudService) : CrudController<Person>(crudService)
{
}
