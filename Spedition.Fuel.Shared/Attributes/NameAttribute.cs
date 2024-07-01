namespace Spedition.Fuel.Shared.Attributes;

public class NameAttribute : Attribute
{
    public NameAttribute(string firstName)
    {
        FirstName = firstName;
    }

    public NameAttribute(string firstName, string secondName)
    {
        FirstName = firstName;
        SecondName = secondName;
    }

    public NameAttribute(string firstName, string secondName, string thirdName)
    {
        FirstName = firstName;
        SecondName = secondName;
        ThirdName = thirdName;
    }

    public NameAttribute(int num)
    {
        ColumnNum = num;
    }

    /// <summary>
    /// Номер столбца в шапке отчета.
    /// </summary>
    public int ColumnNum { get; set; }

    public string FirstName { get; set; }

    public string SecondName { get; set; }

    public string ThirdName { get; set; }
}