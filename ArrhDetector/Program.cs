using ArrhDetector;

int SamplingFreq = 360;
string FileName = "100.csv";
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
Console.WriteLine("Интервалов обнаружено " + WD.FiltredPoints.Count.ToString());