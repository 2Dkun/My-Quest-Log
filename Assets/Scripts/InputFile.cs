using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputFile {
    private string[] lines;
    private int curLine;

    public InputFile(TextAsset input) {
        this.curLine = 0;
        lines = input.text.Split("\n"[0]);
    }

    public string ReadLine(){
        curLine += 1;
        if (curLine > lines.Length)
            curLine = lines.Length;
        return lines[curLine - 1];
    }

    public void resetFile(){
        this.curLine = 0;
    }
}
