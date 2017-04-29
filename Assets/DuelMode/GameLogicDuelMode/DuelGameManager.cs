using System;
﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EZCameraShake;

public class DuelGameManager : MonoBehaviour {
  public int letterPointer = 0;
	public int lettersDestroyed = 0;
  private int currentTyperPlayerId = 0;
  public GameObject[] playerUIPanels;
  private List<List<string>> allRounds;
  public GameObject letterPrefab;
  private bool isFiringOpportunityForFirer;
  private bool isFiringOpportunityForTyper;
  private bool roundStarted;
  private bool gameOver;

  // CODEBLOCK
  public string codeBlock;
  private TextManager textManager;
  public GameObject currentLetter;
  public string[] currentCodeBlock;
  public GameObject UITextBlock;

  // Audio
  public DuelAudioManager duelAudioManager;

  // UI & TYPER DISPLAY
  private TyperDisplayController typerDisplayController;
  private TyperCurrentLettersController typerCurrentLettersController;
  public Transform[] typerTransforms;
  public List<GameObject> lettersForRound = new List<GameObject>();
  public FireIndicatorController[] fireIndicatorControllers;
  public RoleIndicatorController[] roleIndicatorControllers;
  public DeathIndicatorController[] deathIndicatorControllers;


  void Awake() {
    typerDisplayController = UITextBlock.GetComponent<TyperDisplayController>();
    typerCurrentLettersController = GameObject.Find("CurrentLettersDisplay").GetComponent<TyperCurrentLettersController>();
    typerTransforms = typerCurrentLettersController.roundLetterTransforms;

  }

  void Start() {
    roundStarted = false;
    // GetCodeBlockFromFile();
    codeBlock = "private void void void void void void void void void void void void void void void void void void "; // textManager.GetCleanCodeFileAsString();
    currentCodeBlock = SplitCodeblockIntoLetters();
    allRounds = SetUpDuel(currentCodeBlock);
    StartCoroutine(GetReady());
  }

  IEnumerator GetReady() {
    roleIndicatorControllers[currentTyperPlayerId].ShowPrepareToType();
    roleIndicatorControllers[GetFirerPlayerId()].ShowPrepareToFire();
    yield return new WaitForSeconds(2); // REMOVE THIS
    roleIndicatorControllers[currentTyperPlayerId].HideRoleText();
    roleIndicatorControllers[GetFirerPlayerId()].HideRoleText();
    yield return new WaitForSeconds(1);
    DisplayCodeOnTyperUI(currentTyperPlayerId);
    SetUpRound(allRounds);
    StartRound();
  }

  private string[] SplitCodeblockIntoLetters() {
		Debug.Log("inside split code block into letters");
		string[] chars = new string[codeBlock.Length];
		for (var i = 0; i < codeBlock.Length; i++) {
			chars[i] = codeBlock[i].ToString();
		}
		return chars;
	}

  private List<List<string>> SetUpDuel(string[] codeBlock) {
    Debug.Log("typer transform " + typerTransforms.Length);
    // scoreController.ClearPoints();
    var rounds = new List<List<string>>();
    List<string> round = new List<string>();
    var i = 0;
    var totalLettersProcessed = 0;
    while (i < codeBlock.Length) {
      if (round.Count() < typerTransforms.Length) {
        round.Add(codeBlock[i]);
      } else {
        rounds.Add(round);
        round = new List<string>();
        round.Add(codeBlock[i]);
      }
      i++;
      totalLettersProcessed++;
    }
    /*
      Handles edge case where codeblock string is smaller than a single round
      through all transforms in the gallery.
    */
    if (totalLettersProcessed < typerTransforms.Length) {
      rounds.Add(round);
    }
    return rounds;
  }

  /*  ROUND ROUND ROUND ROUND ROUND ROUND ROUND ROUND ROUND ROUND  */
  private void SetUpRound(List<List<string>> rounds) {
    if(rounds.Count() > 0 && rounds.First() != null) {
      InstantiateLetters(rounds.First());
      rounds.RemoveAt(0);
    }
  }

  void StartRound() {
    roundStarted = true;
  }

  void EndRound() {
    roundStarted = false;
    if (allRounds.Count() > 0) {
      SetUpRound(allRounds);
      StartCoroutine("SwitchTurns");
    } else {
      // StartCoroutine(EndGame()); // END GAME HERE
    }
  }

  private IEnumerator SwitchTurns() {
    HideCodeOnTyperUI(currentTyperPlayerId);
    SetCurrentTyperPlayerId();
    roleIndicatorControllers[currentTyperPlayerId].ShowPrepareToType();
    roleIndicatorControllers[GetFirerPlayerId()].ShowPrepareToFire();
    yield return new WaitForSeconds(3);
    roleIndicatorControllers[currentTyperPlayerId].HideRoleText();
    roleIndicatorControllers[GetFirerPlayerId()].HideRoleText();
    DisplayCodeOnTyperUI(currentTyperPlayerId);
    StartRound();
  }


  private void SetCurrentTyperPlayerId() {
    currentTyperPlayerId = currentTyperPlayerId == 0 ? 1 : 0;
  }

  private int GetFirerPlayerId() {
    return currentTyperPlayerId == 0 ? 1 : 0;
  }

  private void DisplayPlayerRoles() {
    // Announce Typer Text at top of Typer's UI
    // Announce Firerer Text at top of Firer's UI
  }

  private void InstantiateLetters(List<string> round) {
    var i = 0;
    foreach (string s in round) {
      var letter = UnityEngine.Object.Instantiate(letterPrefab, typerTransforms[i++]);
      letter.GetComponent<Text>().text = s;
      lettersForRound.Add(letter);
    }
    currentLetter = lettersForRound[0];
    SetCurrentLetterColor(currentLetter);
  }

  void SetCurrentLetterColor(GameObject letter) {
    var textComponent = letter.GetComponent<Text>();
    textComponent.color = Color.red;
    textComponent.fontSize = 38;
    if (textComponent.text == " ") {
      textComponent.text = " "; // TODO: Change to a better character
    }
    else if (textComponent.text == "\n") {
      Debug.Log("NEWLINE CHAR!");
      // reloadNotifier.DisplayReload();
    }
    // set current letter to lerp up and down a little!
  }


  void DisplayCodeOnTyperUI(int playerId) {
    var panelToDisplayCode = playerUIPanels[playerId];
    UITextBlock.transform.SetParent(panelToDisplayCode.transform, false);
    panelToDisplayCode.SetActive(true);
    panelToDisplayCode.GetComponent<Image>().enabled = true;
    typerCurrentLettersController.transform.SetParent(panelToDisplayCode.transform, false);
    typerCurrentLettersController.gameObject.SetActive(true);
  }

  void HideCodeOnTyperUI(int playerId) {
    var panelToDisplayCode = playerUIPanels[playerId];
    typerCurrentLettersController.gameObject.SetActive(false);
    panelToDisplayCode.GetComponent<Image>().enabled = false;
    panelToDisplayCode.SetActive(false);
  }

  void EndGame() {
    gameOver = true;
    Debug.Log("GAME OVER " + gameOver);
    // Draw();
    // if code block is complete, trigger DRAW
  }

  void Update() {
    if(gameOver) return;//GAMEOVER CHECK TOO
    if(!roundStarted) return;
    // Exit KeyCode check here
    if(Input.anyKeyDown) {
      CheckMouseInput();
      CheckKeyboardInput();
    }

  }

  private void CheckMouseInput() {
    if(Input.GetKeyDown(KeyCode.Mouse0)) {
      if(isFiringOpportunityForFirer) {
        KillTyper();
      } else {
        IncorrectClick();
      }
    }
  }

  /* INPUT INPUT INPUT INPUT INPUT INPUT INPUT INPUT INPUT INPUT INPUT INPUT */
  private void CheckKeyboardInput () {
    	if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.LeftShift)) { return; }
      if (Input.GetKeyDown(KeyCode.Space) && isFiringOpportunityForTyper) {
        KillFirer();
      }
    	var targetLetter = currentCodeBlock[letterPointer];
    	if (Input.inputString == targetLetter) {
    		// if (Input.inputString == "\n") {
    		// 	PlayReloadSound();
    		// 	ShotHit();
    		// } else {
    		// 	PlayShotShakeAnim();
    		// 	PlayShotHitSound();
    			CorrectLetter();
    		// }
    	} else {
    		IncorrectLetter();
    	}
    return;
  }

  //If out of time, let other player execute typer :)
  // SHOW FAIL
  // trigger fire opportunity for Firer

  void CorrectLetter() {
    // scoreController.AddPoint();
    // currentLetter.GetComponent<GenerateHitOrMiss>().GenerateHitPrefab();
    AddCurrentLetterToTyperDisplayBlock(currentLetter.GetComponent<Text>().text);
    // if (reloadNotifier.isDisplayed()) reloadNotifier.HideReload(); // Makes sure Reload is toggled off after hitting a space
    Destroy(currentLetter);
    letterPointer++;
    lettersDestroyed++;
    lettersForRound.RemoveAt(0);
    if(lettersForRound.Count() != 0) {
      currentLetter = lettersForRound.First();
      SetCurrentLetterColor(currentLetter);
    } else {
      EndRound();
    }
  }

  private void KillTyper() {
    StartCoroutine("PlayDeathSequence", currentTyperPlayerId);
    Debug.Log("killed typer!");
  }

  private void KillFirer() {
    StartCoroutine("PlayDeathSequence", GetFirerPlayerId());
    Debug.Log("Killed firer");
  }

  IEnumerator PlayDeathSequence(int playerId) {
    duelAudioManager.PlayGunshot();
    yield return new WaitForSeconds(.2f);
    duelAudioManager.PlayBell();
    deathIndicatorControllers[playerId].ShowDeathScreen();
  }

  void AddCurrentLetterToTyperDisplayBlock(string letter) {
    typerDisplayController.SetTyperTextContent(letter);
  }

  void IncorrectLetter() {
    var firerID = currentTyperPlayerId == 0 ? 1 : 0;
    StartCoroutine("DisplayOpportunityToFire", firerID);
  }

  void IncorrectClick() {
    StartCoroutine("DisplayOpportunityToFire", currentTyperPlayerId);
  }

  IEnumerator DisplayOpportunityToFire(int playerId) {
    CheckWhoFired(playerId);
    DisplayFireText(playerId, true);
    yield return new WaitForSeconds(.2f); // TODO: optimize display time here
    ResetFiringOpportunities();
    DisplayFireText(playerId, false);
  }

  void CheckWhoFired(int playerId) {
    if (playerId != currentTyperPlayerId) {
      isFiringOpportunityForFirer = true;
    } else {
      isFiringOpportunityForTyper = true;
    }
  }

  void ResetFiringOpportunities() {
    isFiringOpportunityForFirer = false;
    isFiringOpportunityForTyper = false;
  }

  void DisplayFireText(int playerId, bool toDisplay) {
    fireIndicatorControllers[playerId].DisplayFireText(toDisplay);
  }


}
