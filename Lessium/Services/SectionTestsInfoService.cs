using Lessium.ContentControls;
using Lessium.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lessium.Services
{
    public class SectionTestsInfoService : INotifyPropertyChanged
    {
        private int totalTests = 0;
        private int correctTests = 0;

        private List<ITestControl> wrongTestsControls = new List<ITestControl>();
        private Section section = null;

        #region Properties

        public int TotalTests
        {
            get 
            {
                return totalTests;
            }

            private set
            {
                if (totalTests != value)
                {
                    totalTests = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int CorrectTests
        {
            get 
            {
                return correctTests; 
            }

            private set
            {
                if (correctTests != value)
                {
                    correctTests = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public SectionTestsInfoService(Section section = null)
        {
            UpdateSection(section);

            SectionTestsInfoManager.AddService(this);
        }

        #endregion

        #region Methods

        public void UpdateSection(Section newSection)
        {
            section = newSection;

            if (newSection == null || newSection.ContentType != ContentType.Test)
            {
                Clear();

                return;
            }

            wrongTestsControls.Clear();
            CalculateTests();
        }

        public void CalculateTests()
        {
            int tempTotal = 0;
            int tempCorrect = 0;

            if (section != null)
            {
                foreach (var page in section.Pages)
                {
                    foreach (var item in page.Items)
                    {
                        var testControl = item as ITestControl;

                        if (testControl.TrueAnswers.Count == 0)
                            continue; // There's no answers at all, but CheckAnswers might return true.

                        var correct = testControl.CheckAnswers();

                        tempTotal++;

                        if (correct)
                            tempCorrect++;
                        else
                            wrongTestsControls.Add(testControl); //highlight
                    }
                }
            }

            TotalTests = tempTotal;
            CorrectTests = tempCorrect;
        }

        private void Clear()
        {
            TotalTests = 0;
            CorrectTests = 0;

            wrongTestsControls.Clear();
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(propertyName)));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
