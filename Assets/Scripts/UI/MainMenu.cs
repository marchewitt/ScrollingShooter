using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public void OnNewGameClicked()
        {
            LoadNewGame();
        }

        public void OnSettingsClicked()
        {
            throw new NotImplementedException();
        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }
    
    
        private void LoadNewGame()
        {
            SceneManager.LoadScene("Game");
        }
    }
}
