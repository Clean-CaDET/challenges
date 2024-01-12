# Meaningful Names; Remove Noise Words

## 1. Challenge description
The term "noise words" typically refers to words or phrases in code identifiers that do not contribute to code readability and understandability. Noise words are generic or redundant words that misrepresent or mask the meaning behind the identifier. The student should refactor the code in the [Doctor class](https://github.com/Clean-CaDET/challenges/blob/master/Naming/Doctor.cs) by removing all noise words from identifiers while keeping all meaningful words in place.

## 2. Solving the challenge

Consider the starting code in Doctor.cs:
```csharp
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
```
The list of identifiers includes: `DoctorInfo`, `Id`, `Certificate`, `Certificates`, `HasCertificates`, `certificateSet`, and `c`. Noise words include generic and redundant words that do not support the understanding of the meaning behind an identifier. From the previous list, we can identify the word `Info` as redundant since all classes with fields encapsulate some information (making `Info` redundant). The word `Set`, meant to highlight the collection type, is both redundant and misleading, since the parameter in the method `HasCertificates` is of the `List` type. The solution to the challenge removes these words and results with the following code:

```csharp
public class Doctor
{
    public int Id {get;}
    public ISet<Certificate> Certificates {get;}

    public bool HasCertificates(List<Certificate> certificates)
    {
        foreach (var c in certificates)
        {
            if (!Certificates.Contains(c)) return false;
        }
        return true;
    }
}
```

## 3. Maintainability issue detectors
To automatically identify if the noise words are missing from the solution we define the following _banned words checker_:
```
- Code snippet id: ALL_CODE
- Banned words: info, set, list
```
Apart from the `info` and `set` words, we wish to also catch submissions with the `list` word. The `certificateSet` parameter would stop being misleading if it was named `certificateList`. However, this name also contains a redundant noise word (`list`) and we add that word to the _banned words checker_. When this issue detector fins an issue, the challenge gives the following hint to the learner: _Noise words often repeat the information already present in the variable's type. They also include generic words like "data" that do not introduce new information to the code. Ensure you have located all the identifiers and that none have noise words._

To ensure the student's submission maintains the meaningful words like `Certificates` and `HasCertificates`, we define an additional _required words checker_:
```
- Code snippet id: ALL_CODE
- Required words: Certificates, HasCertificates
```
When this issue detector fins an issue, the challenge gives the following hint to the learner: _Noise words are often generic and do not add new information to the code. We should be careful not to classify meaningful words that describe a concrete technical or business concept as noise words. Ensure you do not remove the meaningful words from the identifiers in the starting code._

## 4. Issue detector limitations
The listed maintainability issue detectors do not protect against other meaningless words. For example, there is nothing stopping the student from submitting the following code:
```csharp
public class NotADoctor
{
    public int IdOrIsIt {get;}
    public ISet<Certificate> Certificates {get;}

    public bool HasCertificates(List<Certificate> certificateCollection)
    {
        foreach (var randomVariableNameHere in certificateCollection)
        {
            if (!Certificates.Contains(randomVariableNameHere)) return false;
        }
        return true;
    }
}
```
The listed code does not contain the banned words and it contains the required words. These issue detectors do not prevent the solution from having odd names (e.g., `IdOrIsIt`). Another, perhaps more serious limitation is showcased in the example below:
```csharp
public class Doctor
{
    public int Id {get;}
    public ISet<Certificate> Certificates {get;}

    public bool SillyName(List<Certificate> certificates)
    {
        foreach (var HasCertificates in certificates)
        {
            if (!Certificates.Contains(HasCertificates)) return false;
        }
        return true;
    }
}
```
The _required words checker_ searches the whole code for the `HasCertificates` identifier. It cannot pinpoint where that name should appear in the code, just that it is located somewhere. The listed submission satisfies the checker despite its maintainability issue. These examples showcase why the current maintainability challenge should be used for formative assessment, not summative assessment.

A more advanced version of our name checkers could be configured to look only at method names of the supplied class by relying on its abstract syntax tree. However, manually configuring such a checker while authoring a maintainability challenge would require more time. Further research is needed to automate this process and create usable authoring controls.
