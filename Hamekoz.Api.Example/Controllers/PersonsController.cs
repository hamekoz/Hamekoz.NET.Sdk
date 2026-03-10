using AutoMapper;

using Hamekoz.Api.Controllers;
using Hamekoz.Api.Example.Dtos;
using Hamekoz.Api.Example.Models;
using Hamekoz.Api.Services;

namespace Hamekoz.Api.Example.Controllers;

public class PersonsController(
        IMapper mapper,
        ICrudService<Person> crudService)
    : CrudController<Person, PersonListItemDto, PersonCreateDto, PersonDetailDto, PersonUpdateDto>(mapper, crudService)
{
}
