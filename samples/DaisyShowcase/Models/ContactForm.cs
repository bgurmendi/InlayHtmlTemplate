using System.ComponentModel.DataAnnotations;

namespace DaisyShowcase.Models;

public class ContactForm
{
    [Display(Name = "Name", Prompt = "Juan")]
    public string Name { get; set; } = "";

    [Display(Name = "Surname", Prompt = "Garcia")]
    public string Surname { get; set; } = "";

    [Display(Name = "Email", Prompt = "juan@example.com")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = "";

    [Display(Name = "Phone", Prompt = "+34 600 123 456")]
    [DataType(DataType.PhoneNumber)]
    public string Phone { get; set; } = "";

    [Display(Name = "Address", Prompt = "Calle Principal 123")]
    public string Address { get; set; } = "";

    [Display(Name = "Zip Code", Prompt = "28001")]
    public string Zip { get; set; } = "";

    [Display(Name = "City", Prompt = "Madrid")]
    public string City { get; set; } = "";

    [Display(Name = "Country", Prompt = "Select country")]
    public string Country { get; set; } = "";

    [Display(Name = "Message", Prompt = "Your message...")]
    [DataType(DataType.MultilineText)]
    public string Message { get; set; } = "";
}
