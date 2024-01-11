# Maintainable Functions; Extract Complex Logic

## 1. Challenge description
Sometimes we have a function that consists of several lines of code that significantly increase the complexity of the entire function. Complex lines of code come in several variants - a chain of method calls, a combination of multiple arithmetic, logical, or relational operations, or deep nesting structures.

When identifying such code, we should ask ourselves whether clarity would be enhanced by extracting a segment of the code into a separate function, where we can hide that complexity behind a good name. Extracting complex expressions into a separate function is a particularly convenient option if we use that logic in multiple places in our code, thereby reducing code duplication. Analyze the [IsAvailable method](https://github.com/Clean-CaDET/challenges/blob/master/Methods/Schedule.cs), find code that is suitable for extraction, and extract it into new functions.

## 2. Solving the challenge

Consider the starting code of the `IsAvailable` method in Schedule.cs:
```csharp
public bool IsAvailable(Doctor doctor, Operation operation)
{
    //Check if doctor is on vacation.
    if (doctor.VacationSlots != null)
    {
        foreach (VacationSlot vacation in doctor.VacationSlots)
        {
            DateTime vacationStart = vacation.StartTime;
            DateTime vacationEnd = vacation.EndTime;

            if (operation.StartTime > operation.EndTime) throw new InvalidOperationException("Invalid operation time frame.");
            //---s1---| vacationStart |---e1---s2---e2---s3---| vacationEnd |---e3---
            if (operation.StartTime <= vacationEnd && operation.EndTime >= vacationStart)
            {
                return false;
            }
        }
    }

    //Check if doctor has operations at the time.
    if (doctor.Operations != null)
    {
        foreach (Operation op in doctor.Operations)
        {
            DateTime opStart = op.StartTime;
            DateTime opEnd = op.EndTime;

            if (operation.StartTime > operation.EndTime) throw new InvalidOperationException("Invalid operation time frame.");
            //---s1---| oldOpStart |---e1---s2---e2---s3---| oldOpEnd |---e3---
            if (operation.StartTime <= opEnd && operation.EndTime >= opStart)
            {
                return false;
            }
        }
    }

    return true;
}
```
Most of the complexity in this code stems from the several levels of nesting. A common heuristic when extracting functions is to observe any comments or whitespace the programmer left in the code, as it often signals a region of cohesive logic. The code above can be transformed by extracting two methods for the two clearly defined regions of the code. The result of these refactorings is below:

```csharp
public bool IsAvailable(Doctor doctor, Operation operation)
{
    if (IsOnVacation(doctor, operation)) return false;
    if (IsInOtherOperation(doctor, operation)) return false;
    return true;
}

private bool IsOnVacation(Doctor doctor, Operation operation)
{
    if (doctor.VacationSlots != null)
    {
        foreach (VacationSlot vacation in doctor.VacationSlots)
        {
            DateTime vacationStart = vacation.StartTime;
            DateTime vacationEnd = vacation.EndTime;

            if (operation.StartTime > operation.EndTime) throw new InvalidOperationException("Invalid operation time frame.");
            //---s1---| vacationStart |---e1---s2---e2---s3---| vacationEnd |---e3---
            if (operation.StartTime <= vacationEnd && operation.EndTime >= vacationStart)
            {
                return true;
            }
        }
    }
    return false;
}

private bool IsInOtherOperation(Doctor doctor, Operation operation)
{
    if (doctor.Operations != null)
    {
        foreach (Operation op in doctor.Operations)
        {
            DateTime opStart = op.StartTime;
            DateTime opEnd = op.EndTime;

            if (operation.StartTime > operation.EndTime) throw new InvalidOperationException("Invalid operation time frame.");
            //---s1---| oldOpStart |---e1---s2---e2---s3---| oldOpEnd |---e3---
            if (operation.StartTime <= opEnd && operation.EndTime >= opStart)
            {
                return true;
            }
        }
    }
    return false;
}
```

The two new functions are still modestly complex. What's worse is that they have a significant portion of duplicate code. Another transformation that would resolve this maintainability issue is to extract the DateTime comparison logic into a shared function. The resulting code is below:

```csharp
public bool IsAvailable(Doctor doctor, Operation operation)
{
    if (IsOnVacation(doctor, operation)) return false;
    if (IsInOtherOperation(doctor, operation)) return false;
    return true;
}

private bool IsOnVacation(Doctor doctor, Operation operation)
{
    if (doctor.VacationSlots != null)
    {
        foreach (VacationSlot vacation in doctor.VacationSlots)
        {
            DateTime newStart = vacation.StartTime;
            DateTime newEnd = vacation.EndTime;

            if (DoesTimeOverlap(operation, newEnd, newStart)) return true;
        }
    }

    return false;
}

private bool IsInOtherOperation(Doctor doctor, Operation operation)
{
    if (doctor.Operations != null)
    {
        foreach (Operation op in doctor.Operations)
        {
            DateTime newStart = op.StartTime;
            DateTime newEnd = op.EndTime;

            if (DoesTimeOverlap(operation, newEnd, newStart)) return true;
        }
    }

    return false;
}

private bool DoesTimeOverlap(Operation operation, DateTime start, DateTime end)
{
    if (operation.StartTime > operation.EndTime) throw new InvalidOperationException("Invalid operation time frame.");
    if (operation.StartTime <= end && operation.EndTime >= start) return true;
    return false;
}
```

## 3. Maintainability issue detectors
To automatically identify if the noise words are missing from the solution we define the following _banned words checker_:
```
-	Code snippet id: Methods.Schedule.IsAvailable
-	Metric name: Cyclomatic complexity
-	Value threshold: 1, 4
```


Basic Metric Checker:
-	Code snippet id: ALL_CODE
-	Metric name: Cyclomatic complexity
-	Value threshold: 1, 6
Basic Metric Checker:
-	Code snippet id: Methods.Schedule
-	Metric name: Number of methods
-	Value threshold: 2, 4


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
So long as the satisfies the identified issue detectors, the students can supply various solutions that do not make sense. This example showcases why the maintainability challenge should be used for formative assessment, and not summative assessment.
