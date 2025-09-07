using AutoMapper;

using Hamekoz.Api.Example.Dtos;
using Hamekoz.Api.Example.Models;

namespace Hamekoz.Api.Example;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Person
        CreateMap<Person, PersonListItemDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        CreateMap<Person, PersonDetailDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateEdad()));

        CreateMap<PersonCreateDto, Person>();
        CreateMap<PersonUpdateDto, Person>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}

public static class DateTimeExtensions
{
    public static int CalculateEdad(this DateOnly fechaNacimiento)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        int edad = hoy.Year - fechaNacimiento.Year;
        if (hoy < fechaNacimiento.AddYears(edad)) edad--;
        return edad;
    }
}
