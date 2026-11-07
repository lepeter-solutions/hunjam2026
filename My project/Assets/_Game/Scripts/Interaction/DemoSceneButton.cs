using UnityEngine;
using UnityEngine.SceneManagement;
namespace HunJam
{
    public sealed class DemoSceneButton : DemoInteractable
    {
        [SerializeField] private string sceneName;
        [SerializeField] private FlightVehicle flight;
        [SerializeField] private FlightSeat seat;
        public void Configure(string scene, FlightVehicle vehicle = null, FlightSeat seating = null)
        { sceneName = scene; flight = vehicle; seat = seating; }
        public override void Interact()
        {
            if (flight != null) { flight.ResetFlight(); seat?.Recenter(); }
            else if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
        }
    }
}
