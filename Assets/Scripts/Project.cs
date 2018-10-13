using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Project {
    
    // Small private class to hold days and months of a year
    [System.Serializable]
    private class yearlyEntry {
        private readonly int MAX_MONTHS = 12;
        private readonly int MAX_DAYS = 31;
        // Fields
        private int year;
        private List<Entry>[,] monthlyEntry;

        // Constructor
        public yearlyEntry(int yr){
            this.year = yr;
            this.monthlyEntry = new List<Entry>[MAX_MONTHS, MAX_DAYS];
            for(int i = 0; i < MAX_MONTHS; i++){
                for(int j = 0; j < MAX_DAYS; j++){
                    this.monthlyEntry[i, j] = new List<Entry>();
                }
            }
        }

        // Methods
        public int getYear()    { return this.year; }
        public List<Entry> getDay(int month, int day){
            if (month > MAX_MONTHS || month < 0)
                return new List<Entry>();
            else {
                return this.monthlyEntry[month - 1, day - 1];
            }
        }

        public void addToDay(Entry e, int month, int day){
            this.monthlyEntry[month - 1, day - 1].Add(e);
        }
    }



    // Fields
    private string projName;
    private int totalEntries;
    private int monID;
    private List<yearlyEntry> yearEntries;



    // Constructor
    public Project(string name, int maxMons) {
        this.projName = name;
        this.totalEntries = 0;
        this.monID = Random.Range(0, maxMons);
        this.yearEntries = new List<yearlyEntry>();
    }



    // Access Functions
    public string getName() { return this.projName; }
    public int getMonID() { return this.monID; }
    public int getEntryAmt() { return this.totalEntries; }
    public List<Entry> getEntries(int year, int month, int day) {
        // Find if the given year has been made. If so, return the entry.
        for (int i = 0; i < this.yearEntries.Count; i++) {
            if (yearEntries[i].getYear() == year) {
                return yearEntries[i].getDay(month, day);
            }
        }

        // Otherwise, return an empty list
        return new List<Entry>();
    }


    // Manipulation Procedures
    public void EntryDeleted() {
        this.totalEntries -= 1;
    }

    public void AddEntry(string text, int maxMons) {
        this.totalEntries += 1;
        Entry E = new Entry(text, maxMons);

        // Find if the current year has been made. If so, add the entry.
        bool newyear = true;
        for (int i = 0; i < this.yearEntries.Count; i++) {
            if (yearEntries[i].getYear() == System.DateTime.Now.Year) {
                newyear = false;
                yearEntries[i].addToDay(E, System.DateTime.Now.Month, System.DateTime.Now.Day);
                break;
            }
        }

        // Otherwise, create a new yearlyEntry and add the new entry to it.
        if (newyear) {
            yearlyEntry Y = new yearlyEntry(System.DateTime.Now.Year);
            Y.addToDay(E, System.DateTime.Now.Month, System.DateTime.Now.Day);
            this.yearEntries.Add(Y);
        }
    }

}
