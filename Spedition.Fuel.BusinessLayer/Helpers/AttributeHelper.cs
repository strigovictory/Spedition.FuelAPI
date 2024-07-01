using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.Shared.Attributes;

namespace Spedition.Fuel.BusinessLayer.Helpers;

public static class AttributeHelper
{
    public static string GetRequiredAttributeValue<TItem, TAttribute>(this PropertyInfo property, Func<TAttribute, string> selector)
        where TItem : class
        where TAttribute : Attribute
    {
        var attributeValue = (property as MemberInfo)?.GetCustomAttribute(typeof(TAttribute)) as TAttribute;
        return selector(attributeValue);
    }

    /// <summary>
    /// Метод для получения атрибута таблицы.
    /// </summary>
    /// <param Name="model">Класс, к которому применен табличный атрибут.</param>
    /// <returns>Наименование таблицы.</returns>
    public static string GetTableAttributeValueName(this Type model)
    {
        string result = string.Empty;

        var temp = model as MemberInfo;

        if (temp != null)
        {
            var tableAttribute = temp.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
            result = tableAttribute?.Name.ToString() ?? model.Name;
        }

        return result;
    }

    public static (string schema, string table) GetTableAttributesValues(this Type model)
    {
        (string schema, string table) result = ("dbo", string.Empty);

        var temp = model as MemberInfo;

        if (temp != null)
        {
            TableAttribute tableAttribute = temp.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
            result = (tableAttribute?.Schema ?? string.Empty, tableAttribute?.Name ?? string.Empty);
        }

        return result;
    }

    /// <summary>
    /// Метод для получения атрибута таблицы.
    /// </summary>
    /// <param Name="model">Класс, к которому применени табличный атрибут.</param>
    /// <returns>Наименование таблицы.</returns>
    public static (string firstName, string secondName, string thirdName) GetNameAttributeValues(this Type model, string propertyName)
    {
        (string firstName, string secondName, string thirdName) result = new (string.Empty, string.Empty, string.Empty);

        MemberInfo prop = model?.GetProperty(propertyName) as MemberInfo;

        if (prop != null)
        {
            var nameAttribute = prop.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
            result = (nameAttribute?.FirstName?.ToString() ?? model.Name, 
                nameAttribute?.SecondName?.ToString() ?? model.Name, 
                nameAttribute?.ThirdName?.ToString() ?? model.Name);
        }

        return result;
    }

    public static string GetNameAttributeValue(this Type model, ReportKind reportKind, string propertyName)
    {
        if (!model.GetProperties().Any(prop => prop.Name == propertyName))
            return string.Empty;

        var prop = model.GetProperty(propertyName) as MemberInfo;

        if (prop?.GetCustomAttribute(typeof(NameAttribute)) != null && prop?.GetCustomAttribute(typeof(NameAttribute)) is NameAttribute nameAttribute)
            return reportKind switch
            {
                ReportKind.monthly => nameAttribute?.FirstName ?? string.Empty,
                ReportKind.daily => nameAttribute?.SecondName ?? string.Empty,
                _ => nameAttribute?.FirstName ?? string.Empty
            };
        else
            return string.Empty;
    }

    public static int GetColumnNumInNameAttrValue(this Type model, string propertyName)
    {
        var result = 0;

        var prop = model?.GetProperty(propertyName) as MemberInfo;

        if (prop != null)
        {
            var nameAttribute = prop.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
            result = nameAttribute?.ColumnNum ?? 0;
        }

        return result;
    }

    public static string GetDisplayAttributeValueName(this Type model)
    {
        string result = string.Empty;

        var modelsMemberInfo = model as MemberInfo;

        if (modelsMemberInfo != null)
        {
            var attributeValue = modelsMemberInfo.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            result = attributeValue?.DisplayName.ToString() ?? model.Name;
        }

        return result;
    }
}
