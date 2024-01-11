# Maintainable Classes; Write Classes that have a Single Responsibility

## 1. Challenge description
Our general goal is to create classes that have a single responsibility, thus adhering to the Single Responsibility Principle. We first need to understand the class's structural stereotype and infer its semantic meaning. We can then analyze the class's structural cohesion and coupling to determine if its suitable for the class's structural stereotype, as well as its semantic cohesion to ensure it matches the class's semantic meaning. During this process we identify areas for improvement and apply _extract method_, _move method_, or _extract class_ refactorings to increase the class's focus. Analyze the [AchievementService class](https://github.com/Clean-CaDET/challenges/blob/master/Classes/Achievements.cs) and ensure it satisfies the single responsibility principle.

## 2. Solving the challenge

Consider the starting code of the `AchievementService` class in Achievements.cs:
```csharp
public class AchievementService
{
    private readonly string _achievementStorageLocation = "C:/MyGame/Storage/Achievements/";
    public void AwardAchievement(int userId, string newAchievementName)
    {
        Achievement newAchievement = LoadAchievement(newAchievementName);
        if (newAchievement == null) throw new Exception("New achievement does not exist in the registry.");

        List<Achievement> unlockedAchievements = LoadUserAchievements(userId, newAchievement);

        CheckIfPrerequisitesUnlocked(newAchievement, unlockedAchievements);

        SaveAchievement(userId, newAchievement);
    }

    private void SaveAchievement(int userId, Achievement newAchievement)
    {
        string newAchievementStorageFormat = newAchievement.Name + ":" + newAchievement.ImagePath + "\n";
        File.AppendAllText(_achievementStorageLocation + userId + ".csv", newAchievementStorageFormat);
    }

    private static void CheckIfPrerequisitesUnlocked(Achievement newAchievement, List<Achievement> unlockedAchievements)
    {
        foreach (var prerequisiteAchievement in newAchievement.PrerequisiteAchievementNames)
        {
            if (!AchievementIsUnlocked(prerequisiteAchievement, unlockedAchievements))
            {
                throw new InvalidOperationException("Prerequisite achievement " + prerequisiteAchievement + " not completed.");
            }
        }
    }

    private static bool AchievementIsUnlocked(string name, List<Achievement> unlockedAchievements)
    {
        foreach (var a in unlockedAchievements)
        {
            if (a.Name.Equals(name)) return true;
        }
        return false;
    }

    private List<Achievement> LoadUserAchievements(int userId, Achievement newAchievement)
    {
        List<Achievement> unlockedAchievements = new List<Achievement>();
        string[] achievements = File.ReadAllLines(_achievementStorageLocation + userId + ".csv");
        foreach (var storedAchievement in achievements)
        {
            string[] achievementElements = storedAchievement.Split(":");
            Achievement a = new Achievement();
            a.Name = achievementElements[0];
            a.ImagePath = achievementElements[1];
            //Check if newAchievement is already unlocked.
            if (a.Name.Equals(newAchievement.Name))
            {
                throw new InvalidOperationException("Achievement " + newAchievement.Name + " is already unlocked!");
            }
            unlockedAchievements.Add(a);
        }

        return unlockedAchievements;
    }

    private Achievement LoadAchievement(string name)
    {
        Achievement newAchievement = null;
        string[] allAchievements = File.ReadAllLines(_achievementStorageLocation + "allAchievements.csv");
        foreach (var achievement in allAchievements)
        {
            string[] achievementElements = achievement.Split(":");
            if (!achievementElements[0].Equals(name)) continue;
            newAchievement = new Achievement();
            newAchievement.Name = achievementElements[0];
            newAchievement.ImagePath = achievementElements[1];
            newAchievement.PrerequisiteAchievementNames = new List<string>();
            //Add ids of prerequisite achievements
            for (int i = 2; i < achievementElements.Length; i++)
            {
                newAchievement.PrerequisiteAchievementNames.Add(achievementElements[i]);
            }
        }

        return newAchievement;
    }
}
```
A common convention is to have _service_ objects act as coordinators of the application, calling on the logic of _domain_ objects and _infrastructure_ objects to deliver a response to the client code. The coordinator structural stereotype is characterised by simple logic in its methods, high efferent coupling, and low structural cohesion. By analyzing the starting code, we find low efferent coupling (`AchievementService` is solely coupled to the `Achievement` class) and high complexity in the functions.

By analyzing the functions we find several different responsibilities that go beyond coordination:

- `LoadAchievement`, `LoadUserAchievements`, and `SaveAchievement` are methods concerned with data storage, a responsibility typically delegated to `Repository` classes.
- `CheckIfPrerequisitesUnlocked` and `AchievementIsUnlocked` are methods that encapsulate business logic that define the rules related to the `Achievement` lifecycle. These responsibilities should be delegated to classes modeling the domain, in this case an achievement collection.

At least two new classes are needed to encapsulate the responsibilities listed above. Using `extract class`, the students create the following code:
```csharp
public class AchievementService
{
    private AchievementFileRepository _repository = new AchievementFileRepository();

    public void AwardAchievement(int userId, string newAchivemenetName)
    {
        Achievement newAchievement = _repository.LoadAchievement(newAchivemenetName);
        if (newAchievement == null) throw new Exception("New achievement does not exist in the registry.");
        AchievementCollection unlockedAchievements = _repository.LoadUnlockedAchievements(userId);

        unlockedAchievements.UnlockAchievement(newAchievement);

        _repository.SaveAchievement(userId, newAchievement);
    }
}

public class AchievementCollection
{
    private List<Achievement> _unlockedAchievements = new List<Achievement>();
    private int _userId;

    public AchievementCollection(List<Achievement> unlockedAchievements, int userId)
    {
        _unlockedAchievements = unlockedAchievements;
        _userId = userId;
    }

    public void UnlockAchievement(Achievement newAchivement)
    {
        ValidateAchievementNotUnlocked(newAchivement);
        CheckIfPrerequisitesUnlocked(newAchivement);
        _unlockedAchievements.Add(newAchivement);
    }

    private void ValidateAchievementNotUnlocked(Achievement newAchievement)
    {
        if(AchievementIsUnlocked(newAchievement.Name, _unlockedAchievements))
        {
            throw new InvalidOperationException("Achievement " + newAchievement.Name + " is already unlocked!");
        }
    }

    private void CheckIfPrerequisitesUnlocked(Achievement newAchievement)
    {
        foreach (var prerequisiteAchievement in newAchievement.PrerequisiteAchievementNames)
        {
            if(!AchievementIsUnlocked(prerequisiteAchievement, _unlockedAchievements)) {
                throw new InvalidOperationException("Prerequisite achievement " + prerequisiteAchievement + " not completed.");
            }
        }
    }

    private static bool AchievementIsUnlocked(string name, List<Achievement> unlockedAchievements)
    {
        foreach (var a in unlockedAchievements)
        {
            if (a.Name.Equals(name)) return true;
        }
        return false;
    }
}

public class AchievementFileRepository
{
    private readonly string _achievementStorageLocation = "C:/MyGame/Storage/Achievements/";
    public void SaveAchievement(int userId, Achievement newAchievement)
    {
        string newAchievementStorageFormat = newAchievement.Name + ":" + newAchievement.ImagePath + "\n";
        File.AppendAllText(_achievementStorageLocation + userId + ".csv", newAchievementStorageFormat);
    }

    public AchievementCollection LoadUnlockedAchievements(int userId)
    {
        List<Achievement> unlockedAchievements = new List<Achievement>();
        string[] achievements = File.ReadAllLines(_achievementStorageLocation + userId + ".csv");
        foreach (var storedAchievement in achievements)
        {
            string[] achievementElements = storedAchievement.Split(":");
            Achievement a = new Achievement(achievementElements[0], achievementElements[1], null);
            unlockedAchievements.Add(a);
        }

        return new AchievementCollection(unlockedAchievements, userId);
    }

    public Achievement LoadAchievement(string name)
    {
        string[] allAchievements = File.ReadAllLines(_achievementStorageLocation + "allAchievements.csv");
        foreach (var achievement in allAchievements)
        {
            string[] achievementElements = achievement.Split(":");
            if (!achievementElements[0].Equals(name)) continue;
            var prerequisiteAchievementNames = new List<string>();
            for (int i = 2; i < achievementElements.Length; i++)
            {
                prerequisiteAchievementNames.Add(achievementElements[i]);
            }
            return new Achievement(achievementElements[0], achievementElements[1], prerequisiteAchievementNames);
        }

        return null;
    }
}

public class Achievement
{
    public string Name { get; private set; }
    public string ImagePath { get; private set; }
    public List<string> PrerequisiteAchievementNames { get; private set; }

    public Achievement(string name, string imagePath, List<string> prerequisiteAchievementNames) {
        Name = name;
        ImagePath = imagePath;
        PrerequisiteAchievementNames = prerequisiteAchievementNames;
    }
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
- Code snippet id: Methods.ScheduleService.IsAvailable
- Metric name: Cyclomatic complexity
- Value threshold: 1, 4
```
This metric checker ensures that the `IsAvailable` method is not overly complex. However, students can extract all the logic into a separate method, thus bypassing the `IsAvailable` complexity check while still producing code with a maintainability issue. We need to define an additional basic metric checker that checks all the functions in the submission. It has the following configuration:
```
- Code snippet id: ALL_CODE
- Metric name: Cyclomatic complexity
- Value threshold: 1, 6
```
A possible side-effect of this restriction, which we observed in the students' submissions, is to create many micro-functions that do not encapsulate any meaningful logic but still manage to reduce the cyclomatic complexity of methods. We subsequently introduced another basic metric checker to ensure that the `ScheduleService` class does not have too many methods:
```
- Code snippet id: Methods.ScheduleService
- Metric name: Number of methods
- Value threshold: 2, 4
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
