using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MathTrainer.Model;

namespace MathTrainer.Persistence;

public class ChildPersistenceService
{
    private readonly string _filePath;

    public ChildPersistenceService(string fileName = "children.json")
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appSpecificFolder = Path.Combine(appDataPath, "MathTrainer");

        if (!Directory.Exists(appSpecificFolder))
        {
            Directory.CreateDirectory(appSpecificFolder);
        }

        _filePath = Path.Combine(appSpecificFolder, fileName);
    }

    public List<Child> LoadChildren()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Child>();
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            var children = JsonSerializer.Deserialize<List<Child>>(json);
            return children ?? new List<Child>();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Fehler beim Kinderlistehochladen... {ex.Message}");
            return new List<Child>();
        }
    }

    public void SaveChildren(List<Child> children)
    {
        try
        {
            string json = JsonSerializer.Serialize(children, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Fehler beim Kinderstellen... {ex.Message}");
        }
    }

    public void AddChild(Child child)
    {
        List<Child> children = LoadChildren();
        children.Add(child);
        SaveChildren(children);
    }

    public Child? GetChildById(Guid id)
    {
        List<Child> children = LoadChildren();
        return children.FirstOrDefault(c => c.Id == id);
    }

    public void UpdateChild(Child updatedChild)
    {
        List<Child> children = LoadChildren();
        int index = children.FindIndex(c => c.Id == updatedChild.Id);
        if (index != -1)
        {
            children[index] = updatedChild;
            SaveChildren(children);
        }
        else
        {
            System.Console.WriteLine($"Kind mit ID {updatedChild.Id} wurde nicht gefunden");
        }
    }

    public void DeleteChild(Guid id)
    {
        List<Child> children = LoadChildren();
        children.RemoveAll(c => c.Id == id);
        SaveChildren(children);
    }
}
