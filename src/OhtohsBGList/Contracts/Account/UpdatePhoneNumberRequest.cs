using System.ComponentModel.DataAnnotations;

namespace OhtohsBGList.Contracts.Account;

public record UpdatePhoneNumberRequest(
    [Required]
    [Phone]
    [MaxLength(20)]
    string PhoneNumber,

    [Required]
    [MaxLength(2048)]
    string Token);
