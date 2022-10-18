using ArrhDetector;

int FileNum = 100;

for (int j = 100; j < 235; j++)
{
    Console.Clear();
    string PathToDatabase = @"D:\J\MIT-BIH Arrhythmia Database\";
    int SamplingFreq = 360;
    string FileName = PathToDatabase + FileNum.ToString() + ".csv";
    Console.WriteLine(FileName);
    string AnnotationFileName = PathToDatabase + FileNum.ToString() + "annotations.txt";
    if (!File.Exists(FileName))
    {
        Console.WriteLine("File " + FileName + "not found");
        FileNum++;
        continue;
    }
    string[] Lines = File.ReadAllLines(FileName);
    double[] Data = new double[Lines.Length];
    for (int i = 1; i < Lines.Length; i++)
    {
        string line = Lines[i];
        string[] parts = line.Split(',');
        Data[i] = int.Parse(parts[1]) - 1024;
    }
    Console.WriteLine("Всего отсчетов : " + Lines.Length);
    WaveDetector WD = new(SamplingFreq);
    for (int i = 0; i < Data.Length; i++)
    {
        WD.Detect(Data, i);
    }

    Console.WriteLine();
    Console.WriteLine("Интервалов обнаружено : " + (WD.FiltredPoints.Count + WD.Arrythmia).ToString());
    Console.WriteLine("Эпизодов аритмии : " + WD.Arrythmia.ToString());
    for (int i = 0; i < WD.ArrytmiaIndexes.Count; i++)
    {
        Console.Write(WD.ArrytmiaIndexes[i].ToString() + " ");
    }

    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("MIT-BIH");
    Lines = File.ReadAllLines(AnnotationFileName);
    Console.WriteLine("Аннотаций : " + Lines.Length.ToString());
    string[] ALines = Lines.Where(s => s.IndexOf('A') > 0 || 
                                       s.IndexOf('V') > 0 ||
                                       s.IndexOf('a') > 0).ToArray();
    Console.WriteLine("Эпизодов аритмии : " + ALines.Length.ToString());
    int startInd = 12;
    int len = 9;
    string[] AIndexes = ALines.Select(s => s.Substring(startInd, len)).ToArray();
    for (int i = 1; i < AIndexes.Length; i++)
    {
        Console.Write(AIndexes[i].Trim() + " ");
    }
    Console.WriteLine();
    Console.WriteLine();
    Console.Write("Введите номер файла, <Enter> для анализа следующего, <Q> для выхода : ");
    string s = Console.ReadLine();
    if (s.ToUpper() == "Q")
    {
        return;
    }
    if (s == "")
    {
        FileNum++;
    }
    else
    {
        int val;
        if (int.TryParse(s, out val) && val > 99 && val < 234)
        {
            FileNum = val;
        }
        else
        {
            FileNum++;
            Console.WriteLine("Ошибка ввода. Номер следующего файла " + FileNum.ToString());
            Console.ReadLine();
        }
    }
}

