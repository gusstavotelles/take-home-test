namespace Fundo.Applications.WebApi.Domain;

public static class LoanStatus
{
    public const string Active = "active";
    public const string Paid = "paid";

    public static bool IsValid(string status) =>
        status is Active or Paid;
}
