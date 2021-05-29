using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagickManager : MonoBehaviour
{
    // Diccionario donde se guardarán todos los magicks existentes
    // La clave (string) será el nombre del magick
    // El valor será una tupla con:
    // List<string> es la lista de elementos que hacen de combinación al magick
    // Bool, true significa que está desbloqueado y se puede usar
    private Dictionary<string, Tuple<List<string>, bool>> magickDict;

    private void Start()
    {
        MakeMagickDict();
    }

    private void MakeMagickDict()
    {
        magickDict = new Dictionary<string, Tuple<List<string>, bool>>();
        List<string> elementList;
        Tuple<List<string>, bool> tuple;

        // Meteor shower
        elementList = new List<string> {"FIR", "EAR", "STE", "EAR", "FIR"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("MeteorShower", tuple);
    }

    // El nombre del magick correspondiente a una lista de elementos determinada
    // Un string vacío significa que no hay magick para esa combinación, o el que hay no está desbloqueado
    public string WrittenMagick(List<string> elementList)
    {
        string magickName = "";

        foreach (KeyValuePair<string, Tuple<List<string>, bool>> magick in magickDict)
        {
            if (!elementList.SequenceEqual(magick.Value.Item1))
                continue;

            if (magick.Value.Item2)
                magickName = magick.Key;
            break;
        }

        return magickName;
    }

    public void CastMagick(string magickName)
    {
        switch (magickName)
        {
            case "MeteorShower":
                break;
        }
    }
}