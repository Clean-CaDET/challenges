# Maintainable Classes; Write Classes that have a Single Responsibility

## 1. Challenge description
Our general goal is to create classes that have a single responsibility, thus adhering to the Single Responsibility Principle. First, we must understand the class's structural stereotype and infer its semantic meaning. We can then analyze the class's structural cohesion and coupling to determine if it is suitable for the class's structural stereotype and its semantic cohesion to ensure it matches the class's semantic meaning. During this process, we identify areas for improvement and apply _extract method_, _move method_, or _extract class_ refactorings to increase the class's focus. Analyze the [AchievementService class](https://github.com/Clean-CaDET/challenges/blob/master/Classes/Achievements.cs) and ensure it satisfies the single responsibility principle.

## 2. Solving the challenge
To complete the challenge, the student needs to have mastered the following knowledge subcomponents:

- _Determine the class’s structural stereotype_,
- _Analyze and improve structural cohesion_, 
- _Analyze and reduce class coupling_
- _Analyze and improve semantic cohesion_.

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
A common convention is to have _service_ objects act as coordinators of the application, calling on the logic of _domain_ objects and _infrastructure_ objects to deliver a response to the client code. The coordinator structural stereotype is characterized by simple logic in its methods, high efferent coupling, and low structural cohesion. By analyzing the starting code, we find low efferent coupling (`AchievementService` is solely coupled to the `Achievement` class) and high complexity in the functions.

By analyzing the functions, we find several different responsibilities that go beyond coordination:

- `LoadAchievement`, `LoadUserAchievements`, and `SaveAchievement` are methods concerned with data storage, a responsibility typically delegated to `Repository` classes.
- `CheckIfPrerequisitesUnlocked` and `AchievementIsUnlocked` are methods that encapsulate business logic that define the rules related to the `Achievement` lifecycle. These responsibilities should be delegated to classes modeling the domain, such as an achievement collection.

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

## 3. Maintainability issue detectors
We define the following _basic metric checkers_ to analyze the metrics of the AchievementService class:
```
- Code snippet id: Classes.AchievementService
- Metric name: Weighted methods per class
- Value threshold: 1, 5

- Code snippet id: Classes.AchievementService
- Metric name: Afferent coupling
- Value threshold: 2, 3

- Code snippet id: Classes.AchievementService
- Metric name: Number of methods defined
- Value threshold: 1, 2
```
The first metric checker ensures that the `AchievementService` class has low complexity (the WMC metric is often calculated as the sum of the cyclomatic complexities of individual methods). The second metric checker checks if the class interacts with a sufficient number of other classes (our ideal solution includes the `Achievement`, the `AchievementCollection`, and the `AchievementFileRepository`). The final metric checker examines if the class has one or two methods, where we expect the other methods to be moved to other classes.

## 4. Issue detector limitations
This example showcases another limitation of our maintainability issue detectors. The `code snippet id` applies either to all code or a specific class or method (as defined by its signature). Since we do not know the exact name the student will give to the `Repository` class (e.g., `AchievementFileRepository` can be `AchievementRepository`) or the `Domain` class (e.g., `AchievementCollection` can be `AchievementInventory`), we cannot create metric checkers tailored specifically for these code snippets. This limitation stops us, for example, from ensuring that the `Repository` class has the three methods we envision as a good solution to the challenge.

One way to bypass this limitation is to give specific instructions to the students on which classes must appear in their solution (or to integrate them into the starting code).
