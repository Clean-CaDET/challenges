using System.Collections.Generic;

namespace Naming;

/// <summary>
/// Unit: Meaningful names; KC: Remove noise words
/// </summary>
public class DoctorInfo
{
    public int Id {get; set;}
    public ISet<Certificate> Certificates {get; set;}

    public bool HasCertificates(List<Certificate> certificateSet)
    {
        foreach (var c in certificateSet)
        {
            if (!Certificates.Contains(c)) return false;
        }
        return true;
    }
}

public class Certificate
{
    public string Name {get; set;}

    public Certificate(string name)
    {
        Name = name;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Certificate other)) return false;
        return other.Name.Equals(Name);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
