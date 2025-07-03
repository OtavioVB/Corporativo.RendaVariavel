namespace Corporativo.RendaVariavel.Domain.BoundedContexts.CustomerContext;

public sealed record Customer
{
    public Customer(string firstName, string lastName, DateTime birthDate, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Email = email;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
}
