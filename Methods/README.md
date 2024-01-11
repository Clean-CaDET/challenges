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
To automatically identify if the student's code contains overly complex methods, we can rely on complexity metrics like cyclomatic complexity. We define the following _basic metric checker_:
```
-	Code snippet id: Methods.ScheduleService.IsAvailable
-	Metric name: Cyclomatic complexity
-	Value threshold: 1, 4
```
This metric checker ensures that the `IsAvailable` method is not overly complex. However, students can extract all the logic into a separate method, thus bypassing the `IsAvailable` complexity check while still producing code with a maintainability issue. We need to define an additional basic metric checker that checks all the functions in the submission. It has the following configuration:
```
-	Code snippet id: ALL_CODE
-	Metric name: Cyclomatic complexity
-	Value threshold: 1, 6
```
A possible side-effect of this restriction, which we observed in the students' submissions, is to create many micro-functions that do not encapsulate any meaningful logic but still manage to reduce the cyclomatic complexity of methods. We subsequently introduced another basic metric checker to ensure that the `ScheduleService` class does not have too many methods:
```
-	Code snippet id: Methods.ScheduleService
-	Metric name: Number of methods
-	Value threshold: 2, 4
```

## 4. Issue detector limitations
The listed maintainability issue detectors do not prevent the student from assigning meaningless names to the classes (covered by the _meaningful names_ unit). Furthermore, the listed metric checkers cannot prevent all odd submissions with maintainability issues. For example, there is nothing stopping the student from submitting the following code:
```csharp
public bool IsAvailable(Doctor doctor, Operation operation)
{
    if (IsOnVacation(doctor, operation) && IsOnVacation(doctor, operation)) return false;
    if (IsInOtherOperation(doctor, operation)) return false;
    return true;
}

// Other methods
```
The previous example shows code that is unlikely to occur in the students' solutions. However, our maintainability issue detectors will not register that this solution has any issues, highlighting the limitation of structural metrics.
