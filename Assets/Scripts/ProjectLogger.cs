using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ProjectLogger : MonoBehaviour {

	private enum menu
	{
		Main, Year, Month, Day, Entry
	};

	/*
	 * Main menu: Select from existing projects or create a new one.
	 * Year menu: Opens up a submenu to ask the user for the year.
	 * Month menu: Cycles through months this year by default. Also have option change to previous years.
	 * 		- Months will be like pc boxes in pokemon and you can select the day by tapping on a square.
	 * 		- In bottom half of screen, there will be an "Add Entry Log" option that allows the user to 
	 * 		add an entry for the current date.
	 * 		- Rest of screen is filled with data such as total amount of entries, etc.
	 * Day menu: Scroll through each entry of selected day. Can swipe left or right to move on to next
	 * 		or previous days.
	 * 		- Can select a single entry to view the full details of the log.
	 * Entry menu: Allows user to either edit the entry contents or delete it.
	 * 		- Deleting the entry brings user back to Day menu.
	 * 		- Have a lined paper theme for background?
	 * 
	 * 
	 * Notes: Months and days go from 1 to whatever so watch out for array out of bounds errors.
	 * 
	 * 
	 * Things to work on:
	 * 		- saving data(can work on later once project is near completion)
	 * 		- deleting an entry
	 * 		- menu user interface
	 * 			- work on main menu(you just create a new project class for now)
	 * 			- allow user to quickly change year
	 * 			- make sprites appear on each day of month
	 * 
	 * 		- you have to use testflight to make sure keyboard input actually works
	 */ 

	// Variables used for user interface
	private menu curMenu, prevMenu;
	public TextMeshProUGUI[] dayNumber;
	public TextMeshProUGUI year, month;
	public GameObject[] dayMonster; // shows sprite of monster on each day
	public TextMeshProUGUI noEntryMsg;

	// Month displays
	public GameObject monthlyContent;
	public GameObject entryPrefab;
	public GameObject entryInput, done, cancel;
	public GameObject dayButtons;
	public GameObject addEntry, prev, next;
	public GameObject monthBox, monthThings, dudeThings;
	public Sprite[] seasons;

	// Day displays
	public GameObject dayStuff;
	public GameObject dayContent;
	public TextMeshProUGUI dayText;
	public TextMeshProUGUI entryAmt;

	// Titlescreen displays
	public TextMeshProUGUI noEntryTitle;
	public GameObject projContent;
	public GameObject projEntryPrefab;
	public GameObject titleThings;
	public GameObject titleBack, titleDone, titleInput, addProj;

	// Entry displays
	public TextMeshProUGUI entryDay, entryLog;
	public GameObject entryStuff, dudeImage;

	// Holds selected date
	private int curYear, curMonth, curDay;
	public GameObject today;
	// Holds all projects made by user
	private List<Project> myProjects;
	private Project curProject;
	// Hold current entry
	EntryPrefab curEntry;
	// Hold list of entries on selected date
	List<Entry> entriesOfDate;

	// Hold each monster sprite
	private List<Sprite> mons;


	// temp vaiables
	public Sprite[] dude;
	public string[] log;

	private bool isTouch;

	// Use this for initialization
	void Start () {
		curMenu = menu.Main;
		// Fill in hashtable for monsters
		mons = new List<Sprite>();
		FillHash();

		SaveManager.Load();

		UpdateProjList();

		curYear = System.DateTime.Now.Year;
		curMonth = System.DateTime.Now.Month;
		UpdateToday();

		DisplayMonthUI(false);
		DisplayEntryUI(false);
		DisplayDayUI(false);
		titleThings.SetActive(true);
		AskProj(false);

		// LOAD FROM SAVE DATA
		myProjects = SaveManager.myProjects;
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log(curMenu);
		if (Input.touchCount > 0 && !isTouch) {
			isTouch = true;
		}
		if (Input.touchCount == 0 && isTouch) {
			if (curMenu == menu.Main) {
				GetProj();
			}
			else if (curMenu == menu.Month || curMenu == menu.Day) {
				prevMenu = curMenu;
				ViewEntry();
			}
			isTouch = false;
		}

	}

	// Month display stuff
	public void DisplayMonthUI(bool display){
		addEntry.SetActive(display);
		prev.SetActive(display);
		next.SetActive(display);
		monthBox.SetActive(display);
		monthThings.SetActive(display);
		dudeThings.SetActive(display);
		if (display)
			UpdateToday();
		else
			today.SetActive(display);
	}

	// Update the display for today
	public void UpdateToday(){
		System.DateTime dt = System.DateTime.Now;
		if (dt.Month != curMonth || dt.Year != curYear) {
			today.SetActive(false);
		}
		else {
			today.SetActive(true);

			int firstDay = (int) new System.DateTime(curYear, curMonth, 1).DayOfWeek;
			int pos = firstDay + dt.Day - 1;
			today.transform.localPosition = new Vector2(-1.384f + ((pos%7)*0.508f), 1.319f + ((pos/7)*-0.505f));
		}
	}

	// Day display stuff
	public void DisplayDayUI(bool display){
		dayStuff.SetActive(display);
		dayContent.SetActive(display);
	}

	// Update display of days based on current month
	private void UpdateMonth() {
		System.DateTime dt = new System.DateTime(curYear, curMonth, 1);
		year.text = curYear.ToString();
		month.text = dt.ToString("MMMM");
		UpdateToday();

		// Change box based on season
		monthBox.GetComponent<SpriteRenderer>().sprite = seasons[(curMonth/3)%4];

		// Update each day in month
		int firstDay = (int)dt.DayOfWeek;
		for (int i = 0; i < firstDay; i++) {
			dayNumber[i].text = "";
			dayMonster[i].GetComponent<SpriteRenderer>().enabled = false;
		}

		for (int i = firstDay; i < System.DateTime.DaysInMonth(curYear, curMonth) + firstDay; i++) {
			int number = i - firstDay + 1;
			dayNumber[i].text = number.ToString();
			dt = new System.DateTime(curYear, curMonth, i - firstDay + 1);
			dayMonster[i].GetComponent<SpriteRenderer>().enabled = true;
			dayMonster[i].GetComponent<SpriteRenderer>().sprite = GetMonster(dt);
		}

	
		for (int i = System.DateTime.DaysInMonth(curYear, curMonth); i < dayNumber.Length; i++) {
			if (i + firstDay >= dayNumber.Length)
				break;
			dayNumber[i + firstDay].text = "";
			dayMonster[i + firstDay].GetComponent<SpriteRenderer>().enabled = false;
		}

		UpdateMonthlyEntries();
	}

	// Creates a new project
	private void CreateProject(string name) {
		Project newProj = new Project(name, this.mons.Count);
		SaveManager.myProjects.Add(newProj);
		curProject = newProj;
	}

	// Returns a monster sprite based on a given date
	private Sprite GetMonster(System.DateTime dt){
		List<Entry> entriesOfDay = curProject.getEntries(dt.Year, dt.Month, dt.Day);
		if (entriesOfDay.Count < 1) {
			return null;
		}
		else {
			int monID = entriesOfDay[0].getMonID();
			//if(mons.ContainsKey(monID))
			if(mons.Count > monID)
				return mons[monID];
			else
				return null;
		}
	}

	// Update the scrollable wheel with entries of the month
	private void UpdateMonthlyEntries(){
		// Get rid of all entries
		foreach (Transform child in monthlyContent.transform) {
			GameObject.Destroy(child.gameObject);
		}

		List<Entry> myEntries = GetMonthEntry();
		if (myEntries.Count > 0) {
			noEntryMsg.enabled = false;
			for (int i = 0; i < myEntries.Count; i++) {
				int thisYear = myEntries[i].getYear();
				int thisMonth = myEntries[i].getMonth();
				int thisDay = myEntries[i].getDay();
				System.DateTime dt = new System.DateTime(thisYear, thisMonth, thisDay);

				GameObject newEntry = Instantiate(entryPrefab) as GameObject;
				EntryPrefab node = newEntry.GetComponent<EntryPrefab>();
				node.dude.sprite = mons[myEntries[i].getMonID()];
				node.log.text = myEntries[i].getLog();
				node.date.text = dt.ToString("D");
				node.dt = dt;
				newEntry.transform.SetParent(monthlyContent.transform);
				newEntry.transform.localScale = Vector3.one;
			}
		}
		else {
			noEntryMsg.enabled = true;
		}
			
	}

	// WE USE A LIST FOR NOW, BUT WHEN WE CARE ABOUT SPECIFIC MONSTERS, WE USE HASH AND THEIR NAMES AS KEYS
	// Read in textfile for monsters and fill in hash accordingly
	private void FillHash() {
		InputFile monTxt = new InputFile(Resources.Load<TextAsset>(@"Monsters"));

		// Variables used for future advanced input 
		List<string> sheetP = new List<string>();
		List<Sprite[]> sheets = new List<Sprite[]>();

		// Determine number of spritesheets
		string s = monTxt.ReadLine();

		string[] split = s.Split(' ');
		int paths = 0;
		int.TryParse(split[split.Length - 1], out paths);
		// Read paths for spritesheets
		for (int i = 0; i < paths; i++) {
			string sheetPath = monTxt.ReadLine();
			Sprite[] sheet = Resources.LoadAll<Sprite>(@sheetPath);

			// Add all sprites in spritesheet as monsters in the hash
			for(int j = 0; j < sheet.Length; j++){
				this.mons.Add(sheet[j]);
			}

			sheets.Add(sheet);
			sheetP.Add(sheetPath);
		}
		monTxt.ReadLine();
	}

	// Changes the current month
	public void ChangeMonth(int change){
		curMonth += change;
		if (curMonth < 1) {
			curMonth = 12;
			curYear -= 1;
		}
		else if (curMonth > 12) {
			curMonth = 1;
			curYear += 1;
		}

		UpdateMonth();
	}

	// Returns all entries in the current month
	public List<Entry> GetMonthEntry(){
		List<Entry> theList = new List<Entry>();

		System.DateTime dt = new System.DateTime(curYear, curMonth, 1);
		int firstDay = (int)dt.DayOfWeek;

		for (int i = firstDay; i < System.DateTime.DaysInMonth(curYear, curMonth) + firstDay; i++) {
			List<Entry> entriesOfDay = curProject.getEntries(curYear, curMonth, i - firstDay + 1);
			for (int j = 0; j < entriesOfDay.Count; j++) {
				theList.Add(entriesOfDay[j].copy());
			}
		}

		return theList;
	}

	// Get text from input and add it as an entry to current project
	public void AddEntry(){
		if (entryInput.GetComponent<TMP_InputField>().text == "") {
		}
		else {
			curProject.AddEntry(entryInput.GetComponent<TMP_InputField>().text, this.mons.Count - 1);
			entryInput.GetComponent<TMP_InputField>().text = "";
			UpdateMonthlyEntries();
			UpdateMonth();
			SaveManager.Save();
		}
		AskEntry(false);
	}

	// Ask user input for an entry
	public void AskEntry(bool getEntry){
		entryInput.GetComponent<TMP_InputField>().text = "";

		entryInput.SetActive(getEntry);
		done.SetActive(getEntry);
		cancel.SetActive(getEntry);
		addEntry.SetActive(!getEntry);
		prev.SetActive(!getEntry);
		next.SetActive(!getEntry);
		dayButtons.SetActive(!getEntry);

		if (getEntry) {
			foreach (Transform child in monthlyContent.transform) {
				GameObject.Destroy(child.gameObject);
			}
		}
		else {
			UpdateMonthlyEntries();
		}
	}

	public void AskProj(bool ask){
		titleInput.SetActive(ask);
		titleDone.SetActive(ask);
		titleBack.SetActive(ask);
		addProj.SetActive(!ask);

		if (ask) {
			noEntryTitle.enabled = false;
			foreach (Transform child in projContent.transform) {
				GameObject.Destroy(child.gameObject);
			}
		}
		else {
			UpdateProjList();
		}
	}

	public void MakeProj(GameObject title){
		CreateProject(title.GetComponent<TMP_InputField>().text);
		title.GetComponent<TMP_InputField>().text = "";
		SaveManager.Save();
		titleThings.SetActive(false);

		DisplayMonthUI(true);
		titleThings.SetActive(false);

		curMenu = menu.Month;
		curYear = System.DateTime.Now.Year;
		curMonth = System.DateTime.Now.Month;
		UpdateMonth();
		UpdateMonthlyEntries();
		AskEntry(false);
	}

	public void UpdateProjList(){
		// Get rid of all entries
		foreach (Transform child in projContent.transform) {
			GameObject.Destroy(child.gameObject);
		}

		if (SaveManager.myProjects.Count > 0) {
			noEntryTitle.enabled = false;
			for (int i = 0; i < SaveManager.myProjects.Count; i++) {
				GameObject newEntry = Instantiate(projEntryPrefab) as GameObject;
				EntryPrefab node = newEntry.GetComponent<EntryPrefab>();
				node.dude.sprite = mons[SaveManager.myProjects[i].getMonID()];
				node.date.text = SaveManager.myProjects[i].getName();
				node.log.text += SaveManager.myProjects[i].getEntryAmt();
				node.count = i;
				newEntry.transform.SetParent(projContent.transform);
				newEntry.transform.localScale = Vector3.one;
			}
		}
		else {
			noEntryTitle.enabled = true;
		}
	}

	public void GetProj(){
		var theClick = EventSystem.current.currentSelectedGameObject;
		if (theClick != null) {
			EntryPrefab e = theClick.GetComponent<EntryPrefab>();
			if (e != null) {
				SelProj(e);
			}
		}
	}

	public void SelProj(EntryPrefab e){
		curMenu = menu.Month;
		curProject = SaveManager.myProjects[e.count];

		DisplayMonthUI(true);
		titleThings.SetActive(false);

		curYear = System.DateTime.Now.Year;
		curMonth = System.DateTime.Now.Month;
		UpdateMonth();
		UpdateMonthlyEntries();
		AskEntry(false);
	}

	public void ToTitle(){
		curMenu = menu.Main;
		DisplayMonthUI(false);
		titleThings.SetActive(true);
		AskProj(false);
	}

	public void DisplayEntryUI(bool disp){
		entryStuff.SetActive(disp);
		dudeImage.SetActive(disp);
	}

	public void ViewEntry(){
		Debug.Log("click");
		var theClick = EventSystem.current.currentSelectedGameObject;
		if (theClick != null) {
			EntryPrefab e = theClick.GetComponent<EntryPrefab>();
			if (e != null) {
				Debug.Log("click2");
				curMenu = menu.Entry;
				DisplayMonthUI(false);
				DisplayDayUI(false);
				DisplayEntryUI(true);

				curEntry = e;
				entryDay.text = e.date.text;
				entryLog.text = e.log.text;
				dudeImage.GetComponent<SpriteRenderer>().sprite = e.dude.sprite;
			}
			else if (theClick.tag == "byDate") {
				Debug.Log("check");
				// Check if the button was avaiable for a day
				System.DateTime dt = new System.DateTime(curYear, curMonth, 1);
				int firstDay = (int)dt.DayOfWeek;
				int clickedDay = -1;
				for (int i = firstDay; i < System.DateTime.DaysInMonth(curYear, curMonth) + firstDay; i++) {
					string target = "day (" + i.ToString() + ")";
					if (theClick.name == target) {
						clickedDay = i - firstDay;
						curDay = i - firstDay;
						List<Entry> entriesOfDate = new List<Entry>();
						List<Entry> entriesOfDay = curProject.getEntries(curYear, curMonth, i - firstDay + 1);
						for (int j = 0; j < entriesOfDay.Count; j++) {
							entriesOfDate.Add(entriesOfDay[j].copy());
						}
						break;
					}
				}

				// Checkpoint 
				if (clickedDay >= 0) {
					// CHANGE DISPLAY MENU TO DATE MODE
					DisplayMonthUI(false);
					DisplayDayUI(true);
					UpdateDailyEntries();
					curMenu = menu.Day;
				}

			}
		}
	}

	public void ToMonth(){
		curMenu = menu.Month;
		DisplayDayUI(false);
		DisplayEntryUI(false);
		DisplayMonthUI(true);
	}

	public void ToLastMenu(){
		DisplayEntryUI(false);
		if (prevMenu == menu.Month) {
			curMenu = menu.Month;
			DisplayMonthUI(true);
			DisplayDayUI(false);
		}
		else {
			curMenu = menu.Day;
			DisplayDayUI(true);
			DisplayMonthUI(false);
		}
	}

	public void DeleteEntry(){
		System.DateTime dt = curEntry.GetComponent<EntryPrefab>().dt;
		string log = curEntry.GetComponent<EntryPrefab>().log.text;
		List<Entry> e = curProject.getEntries(dt.Year, dt.Month, dt.Day);
		for (int i = 0; i < e.Count; i++) {
			if (e[i].equals(dt, log)) {
				e.Remove(e[i]);
				if (prevMenu == menu.Month) {
					UpdateMonthlyEntries();
					UpdateMonth();
				}
				else {
					UpdateDailyEntries();
				}
				ToLastMenu();
				curProject.EntryDeleted();
				SaveManager.Save();
				break;
			}
		}
	}

	// Update the scrollable wheel with entries of the month
	private void UpdateDailyEntries(){
		// Get rid of all entries
		foreach (Transform child in dayContent.transform) {
			GameObject.Destroy(child.gameObject);
		}
			
		List<Entry> myEntries = curProject.getEntries(curYear, curMonth, curDay+1);
		System.DateTime dt = new System.DateTime(curYear, curMonth, curDay+1);
		if (myEntries.Count > 0) {
			for (int i = 0; i < myEntries.Count; i++) {
				GameObject newEntry = Instantiate(entryPrefab) as GameObject;
				EntryPrefab node = newEntry.GetComponent<EntryPrefab>();
				node.dude.sprite = mons[myEntries[i].getMonID()];
				node.log.text = myEntries[i].getLog();
				node.date.text = dt.ToString("D");
				node.dt = dt;
				newEntry.transform.SetParent(dayContent.transform);
				newEntry.transform.localScale = Vector3.one;
			}
		}

		entryAmt.text = "entries: " + myEntries.Count.ToString();
		dayText.text = dt.ToString("D");
	}

	public void updateDay(int change){
		curDay += change;
		if (curDay >= System.DateTime.DaysInMonth(curYear, curMonth)) {
			curDay %= System.DateTime.DaysInMonth(curYear, curMonth);
			curMonth += 1;
			if (curMonth > 12) {
				curMonth = 1;
				curYear += 1;
			}
		}
		else if (curDay < 0) {
			curMonth -= 1;
			if (curMonth < 1) {
				curMonth = 12;
				curYear -= 1;
			} 
			curDay += System.DateTime.DaysInMonth(curYear, curMonth);
		}

		UpdateDailyEntries();
	}
}
