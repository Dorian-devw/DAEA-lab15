using _13Serpentarios.Entidades.DTOs;
using _13Serpentarios.Entidades.Models;
using AutoMapper;

namespace _13Serpentarios.Negocio.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // EspecieSerpiente: Entidad -> DTO de lectura (calcula cantidad de lotes)
        CreateMap<EspeciesSerpiente, EspecieSerpienteDto>()
            .ForMember(dest => dest.CantidadLotes,
                opt => opt.MapFrom(src => src.LotesVenenos.Count));

        // EspecieSerpiente: DTOs de entrada -> Entidad
        CreateMap<EspecieSerpienteCrearDto, EspeciesSerpiente>();
        CreateMap<EspecieSerpienteActualizarDto, EspeciesSerpiente>();

        // LotesVeneno: Entidad -> DTO de lectura (aplana datos de la especie relacionada)
        CreateMap<LotesVeneno, LoteVenenoDto>()
            .ForMember(dest => dest.EspecieNombreComun,
                opt => opt.MapFrom(src => src.Especie.NombreComun))
            .ForMember(dest => dest.EspecieNombreCientifico,
                opt => opt.MapFrom(src => src.Especie.NombreCientifico));

        // LotesVeneno: DTOs de entrada -> Entidad
        CreateMap<LoteVenenoCrearDto, LotesVeneno>();
        CreateMap<LoteVenenoActualizarDto, LotesVeneno>();
    }
}

