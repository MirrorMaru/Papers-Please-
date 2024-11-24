using System;
using System.Collections.Generic;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using xiv.raid.PartyListUtils.Structure;

namespace xiv.raid.DataUtils;

public class ClassJobUtilsSingleton
{
    private static ClassJobUtilsSingleton _instance;
    private static readonly object _lock = new();

    public static ClassJobUtilsSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("ClassJobUtilsSingleton is not initialized.");
            }
            return _instance;
        }
    }
    
    public List<PlayerClassJob> ClassJobs { get; set; } = new();

    private ClassJobUtilsSingleton()
    {
        ExcelSheet < ClassJob > excelSheet= DalamudApi.DataManager.GetExcelSheet<ClassJob>();
        for (int i = 0; i < excelSheet.Count; i++)
        {
            if(excelSheet.GetRowAt(i).JobIndex != 0) ClassJobs.Add(new PlayerClassJob(excelSheet.GetRowAt(i).NameEnglish.ToString().Replace(" ", ""), excelSheet.GetRowAt(i).Abbreviation.ToString()));
        }
        ClassJobs.Add(new PlayerClassJob("Best Rank", "BR"));
    }

    public static void Initialize()
    {
        if (_instance != null)
        {
            throw new InvalidOperationException("ClassJobUtilsSingleton is already initialized.");
        }

        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = new ClassJobUtilsSingleton();
            }
        }
    }

    public PlayerClassJob GetFromName(string name)
    {
        foreach (PlayerClassJob classJob in ClassJobs)
        {
            if (classJob.ClassJobName.Equals(name)) return classJob;
        }
        return null;
    }

    public PlayerClassJob GetFromAbbreviation(string abbreviation)
    {
        foreach (PlayerClassJob classJob in ClassJobs)
        {
            if (classJob.ClassJobAbbreviation.Equals(abbreviation)) return classJob;
        }
        return null;
    }
}
