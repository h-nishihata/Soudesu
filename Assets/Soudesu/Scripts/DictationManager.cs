﻿// https://qiita.com/noir_neo/items/e51f2b503883d9b26c07
// https://github.com/noir-neo/UniSpeech
using UnityEngine;
using UnityEngine.SceneManagement;


namespace UniSpeech.Soudesu
{
    public class DictationManager : MonoBehaviour, ISpeechRecognizer
    {
        public UIController ui;
        private AudioManager audioManager;
        private string prevTranscription = "prevTranscription";

        private Scene scene;
        private float timeSinceLastInput;
        public float timeToReload = 60f;


        void Start()
        {
            SpeechRecognizer.CallbackGameObjectName = gameObject.name;
            SpeechRecognizer.RequestRecognizerAuthorization();
            ui.UpdateButton("認証を待っています", false);
            audioManager = this.gameObject.GetComponent<AudioManager>();
            scene = SceneManager.GetActiveScene();
        }

        private void Update()
        {
            if (timeSinceLastInput < timeToReload)
            {
                timeSinceLastInput += Time.deltaTime;
            }
            else if ((timeSinceLastInput >= timeToReload) || (Input.GetKeyDown(KeyCode.R)))
            {
                this.ReloadScene();
                timeSinceLastInput = 0;
            }
        }

        /// <summary>
        /// マイクから何かしらの入力がある度にこのメソッドが呼ばれる.
        /// </summary>
        /// <param name="transcription">Startボタンを押してからの累計のメッセージが入る.</param>
        public void OnRecognized(string transcription)
        {
            // フィードバック回避.
            if (transcription.Length > 4)
            {
                if((transcription.Substring(transcription.Length - 4) == "そうです") ||
                   (transcription.Substring(transcription.Length - 4) == "ですです") ||
                   (transcription.Substring(transcription.Length - 2) == "です"))
                    return;
            }
                
            // 停止ボタン押下直後になぜか呼ばれてしまう現象回避.
            if (transcription == prevTranscription)
                return;

            timeSinceLastInput = 0;

            // Debug.Log("OnRecognized: " + transcription);
            ui.UpdateText(transcription); // 喋った内容を画面に表示する.
            prevTranscription = transcription;

            audioManager.Play();
        }

        public void OnError(string description)
        {
            ui.onClick = StartRecord;
            ui.UpdateButton("開始", true);
            ui.OnClick(); // 自動で音声認識を開始.
        }

        public void OnAuthorized()
        {
            ui.onClick = StartRecord;
            ui.UpdateButton("開始", true);
            ui.OnClick();
        }

        public void OnUnauthorized()
        {
            ui.UpdateButton("認証に失敗しました", false);
        }

        public void OnAvailable()
        {
            ui.onClick = StartRecord;
            ui.UpdateButton("開始", true);
            ui.OnClick();
        }

        public void OnUnavailable()
        {
            ui.UpdateButton("エラー", false);
        }

        private void StartRecord()
        {
            if (SpeechRecognizer.StartRecord())
            {
                ui.UpdateButton("リセット", true);
                ui.onClick = StopRecord;
            }
        }

        private void StopRecord()
        {
            if (SpeechRecognizer.StopRecord())
            {
                ui.UpdateButton("リセットしています…", false);
                ui.UpdateText("");
            }
        }

        private void ReloadScene()
        {
            //Debug.Log("Reload");
            SceneManager.LoadScene(scene.name);
        }
    }
}