using UnityEngine;

public class Alert
{
    public enum States
    {
        Exclamation,
        Question,
        None,
    }

    private GameObject exclamationPoint;
    private GameObject questionMark;

    public States State
    {
        set
        {
            switch (value)
            {
                case States.Exclamation:
                    exclamationPoint.SetActive(true);
                    questionMark.SetActive(false);
                    break;
                case States.Question:
                    exclamationPoint.SetActive(false);
                    questionMark.SetActive(true);
                    break;
                case States.None:
                    exclamationPoint.SetActive(false);
                    questionMark.SetActive(false);
                    break;
            }
        }
    }

    public Alert(GameObject exclamationPoint, GameObject questionMark)
    {
        this.exclamationPoint = exclamationPoint;
        this.questionMark = questionMark;
    }
}