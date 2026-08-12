using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sucursal360.Web.Domain.Enums;

namespace Sucursal360.Web.ViewModels.AdminBranches;

public class BranchFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El codigo es obligatorio.")]
    [StringLength(20, ErrorMessage = "El codigo no debe superar 20 caracteres.")]
    [RegularExpression(@"^SUC-\d{3}$", ErrorMessage = "Use el formato SUC-###.")]
    [Display(Name = "Codigo")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no debe superar 120 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El proveedor es obligatorio.")]
    [Display(Name = "Proveedor")]
    public PublicDataProvider Provider { get; set; } = PublicDataProvider.Demo;

    [Required(ErrorMessage = "El identificador externo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El identificador externo no debe superar 200 caracteres.")]
    [Display(Name = "Identificador externo")]
    public string ExternalPlaceId { get; set; } = string.Empty;

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<SelectListItem> ProviderOptions { get; set; } =
    [
        new(PublicDataProvider.Demo.ToString(), ((int)PublicDataProvider.Demo).ToString()),
        new(PublicDataProvider.GooglePlaces.ToString(), ((int)PublicDataProvider.GooglePlaces).ToString())
    ];
}
