using System;
using System.Collections.Generic;
using System.Linq;

namespace Naming;

/// <summary>
/// Unit: Meaningful names; KC: Remove noise words
/// </summary>
public class DoctorInfo
{
  public int Id {get;}
  public ISet<Certificate> Certificates {get;}

  public bool HasCertificates(
    List<Certificate> certificateSet)
  {
    foreach (var c in certificateSet)
    {
      if (!Certificates.Contains(c))
        return false;
    }
    return true;
  }
}

public class Certificate
{
    public string Name;

    public Specialization(string name)
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
