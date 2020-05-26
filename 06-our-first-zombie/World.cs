using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class World : MonoBehaviour {

    private static World _instance; // Private reference to the world instance we're going to use.
    public static World Instance { get { return _instance; } } // Public reference to that same instance.

    public int dayStartTime = 240; // 4am.
    public int dayEndTime = 1360; // 10pm - 22 * 60 = 1360
    private int dayLength { get { return dayEndTime - dayStartTime; } }
    private float sunDayRotationPerMinute { get { return 180f / dayLength; } }
    private float sunNightRotationPerMinute { get { return 180f / (1440 - dayLength); } }

    public Transform sun;
    public TextMeshProUGUI clock;

    public int HordeNightFrequency = 7;

    // Check if the current day is divisible by the horde night frequency. If yes, return true.
    public bool IsHordeNight {
        get {
            if (Day % HordeNightFrequency == 0)
                return true;
            else
                return false;
        }
    }

    [Range(4f, 0f)] // The higher this number, the slower the game clock. So reverse the slider order.
    public float ClockSpeed = 1f;

    public int Day = 1;

    // The current time in minutes. We can't call it "Time" because that would conflict with Unity's built in Time function.
    [SerializeField] private int _timeOfDay; // Serialize so we can see in Inspector.
    public int TimeOfDay {

        get { return _timeOfDay; }
        set {

            _timeOfDay = value;
            // There are 1440 minutes in a day wrap our value back around to 0 when it goes over that.
            if (_timeOfDay > 1439) {

                _timeOfDay = 0;
                Day++;

            }

            UpdateClock();

            float rotAmount;

            // The start of the "day" is zero rotation on the sunlight, so that's the most straightforward
            // calculation.
            if (_timeOfDay > dayStartTime && _timeOfDay < dayEndTime) {

                rotAmount = (_timeOfDay - dayStartTime) * sunDayRotationPerMinute;

            // At the end of the "day" we switch to night rotation speed, but in order to keep the rotation
            // seamless, we need to account for the daytime rotation as well.
            } else if (_timeOfDay >= dayEndTime) {
                
                // Calculate the amount of rotation through the day so far.
                rotAmount = dayLength * sunDayRotationPerMinute;
                // Add the rotation since the end of the day.
                rotAmount += ((_timeOfDay - dayStartTime - dayLength) * sunNightRotationPerMinute);

            // Else we're at the start of a new day but because we're still in the same rotation cycle, we need to
            // to account for all the previous rotation since dayStartTime the previous day.
            } else {

                rotAmount = dayLength * sunDayRotationPerMinute; // Previous day's rotation.
                rotAmount += (1440 - dayEndTime) * sunNightRotationPerMinute; // Previous night's rotation.
                rotAmount += _timeOfDay * sunNightRotationPerMinute; // Rotation since midnight.

            }

            sun.eulerAngles = new Vector3(rotAmount, 0f, 0f);

        }
    }

    private void UpdateClock () {

        int hours = TimeOfDay / 60;
        int minutes = TimeOfDay - (hours * 60);

        string dayText;
        if (IsHordeNight)
            dayText = string.Format("<color=red>{0}</color>", Day.ToString());
        else
            dayText = Day.ToString();

        // Adding "D2" to the ToString() command ensures that there will always be two digits displayed.
        clock.text = string.Format("DAY: {0} TIME: {1}:{2}", dayText, hours.ToString("D2"), minutes.ToString("D2"));

    }

    private void Awake() {

        // The first thing this script does is check to see if an instance of the it has already been assigned.
        // If it has, and if that instance is not THIS instance, it deletes itself because we can't have more
        // than one instance.
        if (_instance != null && _instance != this) {

            Debug.LogWarning("More than one instance of World present. Removing additional instance.");
            Destroy(this.gameObject);

        // Else we set the instance to this script. It will now be accessible everywhere through "World.Instance"
        } else
            _instance = this;

    }

    private float secondCounter = 0;

    private void Update() {

        // Increment TimeOfDay every second. Change 1f to speed up/slow down time. (2f would make days twice as long, 0.5f half as long).
        secondCounter += Time.deltaTime;
        if (secondCounter > ClockSpeed) {
            TimeOfDay++;
            secondCounter = 0;
        }

    }

}
