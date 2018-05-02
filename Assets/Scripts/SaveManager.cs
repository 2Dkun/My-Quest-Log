using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class SaveManager {

	public static List<Project> myProjects = new List<Project>();

	public static void Save() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file;

		file = File.Create(Application.persistentDataPath + "/duke.dat");
		bf.Serialize(file, myProjects);

		file.Close();
	}

	public static void Load() {
		BinaryFormatter bf = new BinaryFormatter();

		// Load the first save file if it exists
		if (File.Exists(Application.persistentDataPath + "/duke.dat")) {
			FileStream file = File.Open(Application.persistentDataPath + "/duke.dat", FileMode.Open);
			myProjects = (List<Project>)bf.Deserialize(file);
			file.Close();
		}
	}

}
