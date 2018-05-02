using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Entry {

	// Fields
	int day, month, year;
	string log;

	// maybe use this
	int monsterID;

	// Constructor
	public Entry(string text, int maxMons) {
		this.log = text;
		this.day = System.DateTime.Now.Day;
		this.month = System.DateTime.Now.Month;
		this.year = System.DateTime.Now.Year;
		// Generate random event/monster
		this.monsterID = Random.Range(0, maxMons);
	}

	// Access Functions
	public int getDay()		{ return this.day; }
	public int getMonth()	{ return this.month; }
	public int getYear()	{ return this.year; }
	public string getLog()	{ return this.log; }
	public int getMonID()	{ return this.monsterID; }

	// Manipulation Procedures
	public void setDay(int i)		{ this.day = i; }
	public void setMonth(int i)		{ this.month = i; }
	public void setYear(int i)		{ this.year = i; }
	public void setLog(string i)	{ this.log = i; }
	public void setMonID(int ID)	{ this.monsterID = ID; }

	// Other Functions 
	public Entry copy(){
		Entry clone = new Entry(this.log, this.monsterID);
		clone.setDay(this.day);
		clone.setMonth(this.month);
		clone.setYear(this.year);
		clone.setMonID(this.monsterID);
		return clone;
	}

	public bool equals(System.DateTime dt, string loggy){
		if (this.day != dt.Day)
			return false;
		if(this.month != dt.Month)
			return false;
		if (this.year != dt.Year)
			return false;
		if (this.log != loggy)
			return false;
		return true;
	}
}
