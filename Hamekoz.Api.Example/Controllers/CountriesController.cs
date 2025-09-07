using AutoMapper;

using Hamekoz.Api.Controllers;
using Hamekoz.Api.Example.Models;
using Hamekoz.Api.Services;

namespace Hamekoz.Api.Example.Controllers;

public class CountriesController(
        IMapper mapper,
        ICrudService<Country> crudService)
    : CrudController<Country>(mapper, crudService)
{
}
