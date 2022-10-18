using System.Drawing;

namespace ArrhDetector
{
    class WaveDetector
    {
        private int CurrentInterval;
        public double DetectLevel = 5;
        private const double MinDetectLevel = 5;
        private int LockInterval = 60;
        private const int NoWaveInterval1 = 600;
        private const int NoWaveInterval2 = 1000;
        public double MaxD;
        private const int NNArrSize = 10000;
        private Point[] NNPointArr;
        private int NNPointIndex;
        private int PrevInterval;
        private int[] NNArray;
        private int NNIndex;
        private int NumOfIntervalsForAver;
        private const int MaxNumOfIntervalsForAver = 10;
        public List<int> FiltredPoints;
        private readonly int _samplingFrequency;
        private const double _detectLevelCoeff = 0.7;
        private int _currentInterval;

        private double CurrentValue;
        public EventHandler<WaveDetectorEventArgs> OnWaveDetected;

        public int Arrythmia;
        public List<int> ArrytmiaIndexes = new List<int>();

        public WaveDetector(int samplingFrequency)
        {
            NNPointArr = new Point[NNArrSize];
            NNArray = new int[NNArrSize];
            FiltredPoints = new List<int>();
            _samplingFrequency = samplingFrequency;
        }

        protected virtual void NewWaveDetected()
        {
            WaveDetectorEventArgs args = new()
            {
                WaveCount = FiltredPoints.Count,
                Amplitude = CurrentValue,
                Interval = _currentInterval,
                Arrithmia = Arrythmia
            };
            OnWaveDetected?.Invoke(this, args);
        }

        public void Reset()
        {
            NNPointIndex = 0;
            NNIndex = 0;
            FiltredPoints.Clear();
            DetectLevel = MinDetectLevel;
            Arrythmia = 0;
            ArrytmiaIndexes.Clear();
        }

        public int GetCurrentPulse()
        {
            if (NNIndex == 0)
            {
                return 0;
            }
            if (NNIndex < NumOfIntervalsForAver)
            {
                NumOfIntervalsForAver = NNIndex;
            }
            int sum = 0;
            for (int i = 0; i<NumOfIntervalsForAver; i++)
            {
                sum += NNArray[(NNIndex - 1 - i) ];
            }
            sum /= NumOfIntervalsForAver;
            double d = sum;
            d /= _samplingFrequency; 
            d = 60 / d;
            return (int)Math.Round(d);
        }

        public double Detect(double[] DataArr, int Ind)
        {
            CurrentInterval++;
            if (CurrentInterval == NoWaveInterval1)
            {
                DetectLevel /= 2;
            }
            if (CurrentInterval > NoWaveInterval2)
            {
                DetectLevel /= 2;
                NumOfIntervalsForAver = 0;
            }
            DetectLevel = Math.Max(DetectLevel, MinDetectLevel);

            if (CurrentInterval < LockInterval)
            {
                return DetectLevel;
            }

            CurrentValue = DataArr[Ind - 1];
            if (CurrentValue > DetectLevel)
            {
                MaxD = Math.Max((int)CurrentValue, MaxD);
                if (MaxD > CurrentValue)
                {
                    int tmpNN = 0;
                    NNPointArr[NNPointIndex].X = Ind - 1;
                    NNPointArr[NNPointIndex].Y = (int)MaxD;
                    if (NNPointIndex > 0)
                    {
                        tmpNN = NNPointArr[NNPointIndex].X - NNPointArr[NNPointIndex - 1].X;
                    }
                    if (Filter25percent(tmpNN, Ind))
                    {
                        NNArray[NNIndex] = tmpNN;
                        _currentInterval = tmpNN;
                        NNIndex++;
                        NumOfIntervalsForAver++;
                        NumOfIntervalsForAver = Math.Min(NumOfIntervalsForAver, MaxNumOfIntervalsForAver);
                        FiltredPoints.Add(Ind - 1);
                        LockInterval = tmpNN / 2;
                        NewWaveDetected();
                    }
                    CurrentInterval = 0;
                    NNPointIndex++;
                    DetectLevel = MaxD * _detectLevelCoeff;
                    MaxD = 0;
                }
            }
            return DetectLevel;
        }

        private bool Filter25percent(int NewInterval, int index)
        {
            const int LoLimit = 50;  //ms - 240 уд / мин 
            const int HiLimit = 400; //ms - 30  уд / мин
            const int MinNumberForArrythmia = 4;
            if (NewInterval < LoLimit) 
            {
                return false;
            }
            if (NewInterval > HiLimit)
            {
                return false;
            }
            int PrevInt = PrevInterval;
            PrevInterval = NewInterval;
            if (NNPointIndex < MinNumberForArrythmia)
            {
                return true;
            }
            if (NewInterval > PrevInt + PrevInt / 2 || NewInterval < PrevInt - PrevInt / 2)
            {
                if (NNPointIndex > 0)
                {
                    ArrytmiaIndexes.Add(NNPointArr[NNPointIndex - 1].X);
                }
                Arrythmia++;
                return false;
            }
            return true;
        }
    }
}
