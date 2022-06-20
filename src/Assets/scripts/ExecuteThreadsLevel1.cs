using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class ExecuteThreadsLevel1 : MonoBehaviour {

	//	public LogManager.LogData loggingData = new LogManager.LogData ();

	// --- IMAGE SIMULATION ---

	public GameObject scrollRect;


	public GameObject simulationImagePrefab;
	public GameObject simulationErrorPrefab;
	public GameObject layoutPanel;
	public Text stepsIndicator;

	public Sprite dogSprite;
	public Sprite workerSprite;
	public Sprite displayErrorSprite;
	public Sprite[] itemsSprites;
	public Sprite[] actionsSprites;

	// ------------------------

	ToolboxManager manager;
	GameObject disablePanel;
	ProgressBar bar;
	ScrollRect simulationScrollRect;

	public GameObject runButton;
	public GameObject stopButton;

	// private Timer timer;
	private int numActions;
	private string toPrint;
	public GenerateTasks genTasks;

	Transform[] blocks_t1;

	bool stop;
	bool err;
	bool paused;
	bool lost;

	bool t1_checkedin;
	bool t1_checkedout;
	bool t1_has_brush;
	bool t1_has_clippers;
	bool t1_has_conditioner;
	bool t1_has_dryer;
	bool t1_has_scissors;
	bool t1_has_shampoo;
	bool t1_has_station;
	bool t1_has_towel;

	bool t1_needs_cut;
	bool t1_needs_dry;
	bool t1_needs_wash;
	bool t1_needs_groom;
	bool t1_did_cut;
	bool t1_did_dry;
	bool t1_did_wash;
	bool t1_did_groom;

	string returnErrMsg = "> ERROR: You are trying to return a resource you don't have.";
	string acquireErrMsg = "> ERROR: You are trying to acquire a resource you already have.";

	void Start () {
		stop = false;
		err = false;
		paused = false;
		lost = false;

		t1_checkedin = false;
		t1_checkedout = false;

		t1_has_brush = false;
		t1_has_clippers = false;
		t1_has_conditioner = false;
		t1_has_dryer = false;
		t1_has_scissors = false;
		t1_has_shampoo = false;
		t1_has_station = false;
		t1_has_towel = false;

		manager = GameObject.Find ("_SCRIPTS_").GetComponent<ToolboxManager> ();
		// timer = GameObject.FindObjectOfType<Timer> ();
		disablePanel = GameObject.Find ("DisablePanel");
		bar = GameObject.Find ("RadialProgressBar").GetComponent<ProgressBar> ();
		simulationScrollRect = scrollRect.transform.GetComponent<ScrollRect> ();

		try {
			disablePanel.SetActive (false);
		} catch {
			Debug.Log ("Disable Panel can't be found.");
		}
	}

	public void ExecuteThreads () {
		LogManager.instance.logger.sendChronologicalLogs("RunLevel01Thread", "", LogManager.instance.UniEndTime().ToString());

		clearAllClones ();
		clearVerticalLayout ();

		t1_did_cut = false;
		t1_did_dry = false;
		t1_did_wash = false;
		t1_did_groom = false;

		// ----- SET UP FOR BRUCE, CUSTOMER FOR LEVEL 1 -----
		t1_needs_cut = true;
		t1_needs_dry = false;
		t1_needs_wash = false;
		t1_needs_groom = false;

		// ------ START EXECUTE THREADS -------

		try {
			GameObject.Find ("InformationPanel").SetActive (false);
		} catch { }

		try {

			GameObject.Find ("AgendaPanel").SetActive (false);
		} catch { }

		stop = false;
		err = false;
		paused = false;
		lost = false;

		t1_checkedin = false;
		t1_checkedout = false;

		t1_has_brush = false;
		t1_has_clippers = false;
		t1_has_conditioner = false;
		t1_has_dryer = false;
		t1_has_scissors = false;
		t1_has_shampoo = false;
		t1_has_station = false;
		t1_has_towel = false;

		try {
			// disable all other functionalities
			disablePanel.SetActive (true);
		} catch {
			Debug.Log ("Cannot enable DisablePanel");
		}

		// switch to stop button
		runButton.transform.SetAsFirstSibling ();

		// ------------------------ READING THREAD 1 ------------------------

		// int thread1_whilesChildren = 0;

		// retrieving the objects (blocks) current in thread 1
		blocks_t1 = GetActionBlocks_MultiThreads ("1");

		// this structure will store the text lines to display
		List<string> blocks_names_t1 = new List<string> ();
		List<GameObject> simulationImagesToDisplay = new List<GameObject> ();

		int i = 0;

		foreach (Transform child in blocks_t1) {

			if (child.GetComponent<Draggable> ().typeOfItem == Draggable.Type.ACTION) {

				//	Debug.Log ("TYPE ACTION");

				// action block is a GET action
				if (blocks_t1[i].transform.GetComponentInChildren<Text> ().text == "get") {

					string resource = blocks_t1[i].transform.Find ("Dropdown").Find ("Label").GetComponent<Text> ().text;

					if (resource == "[null]") {
						terminateSimulation ("Please select a resource to acquire.");
						manager.showError ("Please select a resource to acquire.");
						return;

					} else {

						blocks_names_t1.Add ("acquire ( " + resource + " );");
						InputWorkerData inpt = new InputWorkerData { action = resource, typeOf = "Acquire" };
						LogManager.instance.logger.sendInputWorkerOne(resource, "Acquire", LogManager.instance.UniEndTime().ToString());
						GameLogData.inputList_t1.Add (inpt);

						i++;

						// create new object from prefab
						GameObject newItem = Instantiate (simulationImagePrefab) as GameObject;
						newItem.transform.Find ("Icon").GetComponent<Image> ().sprite = workerSprite;
						newItem.transform.Find ("AcqRet").GetComponent<Image> ().sprite = actionsSprites[0];

						Sprite item;

						if (resource == "brush")
							item = itemsSprites[0];
						else if (resource == "clippers")
							item = itemsSprites[1];
						else if (resource == "cond.")
							item = itemsSprites[2];
						else if (resource == "dryer")
							item = itemsSprites[3];
						else if (resource == "scissors")
							item = itemsSprites[4];
						else if (resource == "shampoo")
							item = itemsSprites[5];
						else if (resource == "station")
							item = itemsSprites[6];
						else if (resource == "towel")
							item = itemsSprites[7];
						else
							item = displayErrorSprite;

						newItem.transform.Find ("ItemAction").GetComponent<Image> ().sprite = item;
						newItem.transform.Find ("ActionText").GetComponent<Text> ().text = "get(" + resource + ");";
						simulationImagesToDisplay.Add (newItem);

					}

					// action block is a RETURN action
				} else if (blocks_t1[i].transform.GetComponentInChildren<Text> ().text == "ret") {

					string resource = blocks_t1[i].transform.Find ("Dropdown").Find ("Label").GetComponent<Text> ().text;

					if (resource == "[null]") {

						terminateSimulation ("Please select a resource to return.");
						manager.showError ("Please select a resource to return.");
						return;

					} else {

						blocks_names_t1.Add ("return ( " + resource + " );");
						InputWorkerData inpt = new InputWorkerData { action = resource, typeOf = "Return" };
						LogManager.instance.logger.sendInputWorkerOne(resource, "Return", LogManager.instance.UniEndTime().ToString());
						GameLogData.inputList_t1.Add (inpt);

						//						LogData.inputList_t1.Add ("Return: " + resource);

						i++;

						// create new object from prefab
						GameObject newItem = Instantiate (simulationImagePrefab) as GameObject;
						newItem.transform.Find ("Icon").GetComponent<Image> ().sprite = workerSprite;
						newItem.transform.Find ("AcqRet").GetComponent<Image> ().sprite = actionsSprites[1];

						Sprite item;

						if (resource == "brush")
							item = itemsSprites[0];
						else if (resource == "clippers")
							item = itemsSprites[1];
						else if (resource == "cond.")
							item = itemsSprites[2];
						else if (resource == "dryer")
							item = itemsSprites[3];
						else if (resource == "scissors")
							item = itemsSprites[4];
						else if (resource == "shampoo")
							item = itemsSprites[5];
						else if (resource == "station")
							item = itemsSprites[6];
						else if (resource == "towel")
							item = itemsSprites[7];
						else
							item = displayErrorSprite;

						newItem.transform.Find ("ItemAction").GetComponent<Image> ().sprite = item;
						newItem.transform.Find ("ActionText").GetComponent<Text> ().text = "return(" + resource + ");";
						simulationImagesToDisplay.Add (newItem);

					}

				} else {

					String action = blocks_t1[i].transform.GetComponentInChildren<Text> ().text;

					blocks_names_t1.Add (action + ";");
					InputWorkerData inpt = new InputWorkerData { action = action, typeOf = "Action" };
					LogManager.instance.logger.sendInputWorkerOne(action, "Action", LogManager.instance.UniEndTime().ToString());
					GameLogData.inputList_t1.Add (inpt);

					//					LogData.inputList_t1.Add ("Action: " + action);

					i++;

					GameObject newItem = Instantiate (simulationImagePrefab) as GameObject;

					if (action == "checkin") {

						//Debug.Log ("CHECKING IN");

						newItem.transform.Find ("Icon").GetComponent<Image> ().sprite = workerSprite;
						newItem.transform.Find ("ItemAction").GetComponent<Image> ().sprite = dogSprite;
						newItem.transform.Find ("AcqRet").GetComponent<Image> ().sprite = actionsSprites[0];

					} else if (action == "checkout") {

						// Debug.Log ("CHECKING OUT");

						newItem.transform.Find ("Icon").GetComponent<Image> ().sprite = workerSprite;
						newItem.transform.Find ("ItemAction").GetComponent<Image> ().sprite = dogSprite;
						newItem.transform.Find ("AcqRet").GetComponent<Image> ().sprite = actionsSprites[1];

					} else {

						// create new object from prefab (single action)
						newItem.transform.Find ("Icon").GetComponent<Image> ().sprite = dogSprite;

						Sprite item;

						if (action == "cut")
							item = actionsSprites[2];
						else if (action == "dry")
							item = actionsSprites[3];
						else if (action == "wash")
							item = actionsSprites[4];
						else if (action == "groom")
							item = actionsSprites[5];
						else
							item = displayErrorSprite;

						newItem.transform.Find ("ItemAction").GetComponent<Image> ().sprite = item;
					}
					newItem.transform.Find ("ActionText").GetComponent<Text> ().text = action + ";";
					simulationImagesToDisplay.Add (newItem);

				}

			} else if (child.GetComponent<Draggable> ().typeOfItem == Draggable.Type.IFSTAT) {

				//Debug.Log ("TYPE IFSTAT");

			} else if (child.GetComponent<Draggable> ().typeOfItem == Draggable.Type.WHILELOOP) {

				//Debug.Log ("TYPE WHILELOOP");
			}
		}

		if (blocks_t1.Length < 1) {

			manager.showError ("There are no actions to run.");

			terminateSimulation ("There are no actions to run.");
			return;
		}

		try {
			if ((blocks_names_t1[0].Substring (0, 7) != "checkin" /*&& blocks_names_t1 [0].Substring (11, 17) != "pickup"*/ )) {

				manager.showError ("Remember to always check-in your costumer first!");

				terminateSimulation ("Remember to always check-in your costumer first!");
				return;
			}

		} catch {

			manager.showError ("Remember to always check-in your costumer first!");

			terminateSimulation ("Remember to always check-in your costumer first!");
			return;
		}

		try {

			if ((blocks_names_t1[blocks_names_t1.Count - 1].Substring (0, 8) != "checkout")) {

				manager.showError ("Remember to always check-out your costumer when you're done!");

				terminateSimulation ("Remember to always check-out your costumer when you're done!");
				return;
			}
		} catch {
			manager.showError ("Remember to always check-out your costumer when you're done!");

			terminateSimulation ("Remember to always check-out your costumer when you're done!");
			return;
		}

		if (!err)
			StartCoroutine (printThreads_single (blocks_names_t1, simulationImagesToDisplay, 14)); // List<>, speed/step

		print ("Execute");
		// AnalyticsResult result = Analytics.CustomEvent ("Test");
		// // This should print "Ok" if the event was sent correctly.
		// Debug.Log (result);

		// print (AnalyticsSessionInfo.userId);
		// print (AnalyticsSessionInfo.sessionId);
		// print (AnalyticsSessionInfo.sessionElapsedTime);
		// print (AnalyticsSessionInfo.sessionState);
		//		print (LogManager.instance.EndTimer ());

	}

	private Transform[] GetActionBlocks_MultiThreads (String tabNum) {

		//get children in drop area for thread

		string path = "";

		if (tabNum == "1")
			path = "Tab1/ScrollRect/Holder/DropAreaThread1";
		else
			path = "Tab2/ScrollRect/Holder/DropAreaThread2";

		Debug.Log ("children: " + GameObject.Find (path).transform.childCount);
		int childCount = GameObject.Find (path).transform.childCount;

		Transform[] threadChildren = new Transform[childCount];

		for (int i = 0; i < childCount; i++) {

			threadChildren[i] = GameObject.Find (path).transform.GetChild (i);
		}

		return threadChildren;
	}

	public void terminateSimulation (string error) {
		LogManager.instance.logger.sendChronologicalLogs("TerminateLevel1", "", LogManager.instance.UniEndTime().ToString());

		GameLogData.failedReason = error;
		LogManager.instance.failCount++;
		// LogData.levelSteps = blocks_t1.Length;
		GameLogData.failedAttempts = LogManager.instance.failCount;
		GameLogData.levelClearedTime = LogManager.instance.EndTimer ();
		GameLogData.levelClearAmount = bar.currentAmount;
		LogManager.instance.CreateLogData ();

		err = true;
		lost = true;
		stop = true;

		try {
			disablePanel.SetActive (false);
		} catch {
			Debug.Log ("Cannot disable DisablePanel.");
		}

		runButton.transform.SetAsLastSibling ();
		bar.LoadingBar.GetComponent<Image> ().fillAmount = 0;
		scrollToBottom ();
	}

	IEnumerator printThreads_single (List<string> b1, List<GameObject> b3, int speed) {

		bar.currentAmount = 0;

		// int step_counter = 1;
		int t1_curr_index = 0;

		bool t1_canPrint = true;

		int j = 0;

		while ((t1_curr_index < b1.Count)) {

			if (bar.currentAmount < 100) {

				bar.currentAmount += speed;
				bar.LoadingBar.GetComponent<Image> ().fillAmount = bar.currentAmount / 100;

			} else {
				LogManager.instance.logger.sendChronologicalLogs("Level01Lost", "", LogManager.instance.UniEndTime().ToString());

				manager.gameLost ();
				//logging
				GameLogData.isLevelCleared = false;
				//	LogData.levelSteps = j;
				GameLogData.levelClearedTime = LogManager.instance.EndTimer ();
				GameLogData.levelClearAmount = bar.currentAmount;
				GameLogData.failedReason = "Times up! GameLost";
				LogManager.instance.failCount++;
				GameLogData.failedAttempts = LogManager.instance.failCount;
				//				StartCoroutine (LogManager.instance.SendLogJson ());
				LogManager.instance.CreateLogData ();

				LogManager.instance.isQuitLogNeed = false;

				stop = true;
				paused = true;
				lost = true;

				stopButton.transform.GetComponent<Button> ().interactable = false;

				yield return 0;
			}

			if (stop) {

				if (!paused) {

					try {

						disablePanel.SetActive (false);

					} catch {

						Debug.Log ("Cannot disable DisablePanel");
					}

					runButton.transform.SetAsLastSibling ();

				}

				bar.LoadingBar.GetComponent<Image> ().fillAmount = 0;

				break;
				// yield break;
				// yield return 0;

			} else {

				stepsIndicator.text = "" + (j + 1);

				// ------------------------------  THREAD 1 ------------------------------

				try {

					// {"[null]", "brush" ,"clippers" , "cond.", "dryer", "scissors", "shampoo", "station", "towel"};

					if (b1[t1_curr_index].Substring (0, 3) == "acq") {

						// acquiring resource
						switch (b1[t1_curr_index].Substring (10, 5)) {

							case "brush":

								int output1 = acquire (ref t1_has_brush);
								t1_canPrint = true;

								if (output1 < 0) {
									resError (acquireErrMsg); // ERROR: You are trying to acquire a resource you already have.";
								}

								break;

							case "clipp":

								int output2 = acquire (ref t1_has_clippers);
								t1_canPrint = true;

								if (output2 < 0) {
									resError (acquireErrMsg);
								}

								break;

							case "cond.":

								int output3 = acquire (ref t1_has_conditioner);
								t1_canPrint = true;

								if (output3 < 0) {
									resError (acquireErrMsg);
								}

								break;

							case "dryer":

								int output4 = acquire (ref t1_has_dryer);
								t1_canPrint = true;

								if (output4 < 0) {
									resError (acquireErrMsg);
								}

								break;

							case "sciss":

								int output5 = acquire (ref t1_has_scissors);
								t1_canPrint = true;

								if (output5 < 0) {
									resError (acquireErrMsg);
								}

								break;

							case "shamp":

								int output6 = acquire (ref t1_has_shampoo);
								t1_canPrint = true;

								if (output6 < 0) {
									resError (acquireErrMsg);
								}

								break;

							case "stati":

								int output7 = acquire (ref t1_has_station);
								t1_canPrint = true;

								if (output7 < 0) {
									resError (acquireErrMsg);
								}

								break;

							case "towel":

								int output8 = acquire (ref t1_has_towel);
								t1_canPrint = true;

								if (output8 < 0) {
									resError (acquireErrMsg);
								}

								break;
						}

					} else if (b1[t1_curr_index].Substring (0, 3) == "ret") {

						// returning resource
						switch (b1[t1_curr_index].Substring (9, 5)) {

							case "brush":

								int output1 = return_res (ref t1_has_brush);

								if (output1 < 0) {
									resError (returnErrMsg);
								}

								break;

							case "clipp":

								int output2 = return_res (ref t1_has_clippers);

								if (output2 < 0) {
									resError (returnErrMsg);
								}

								break;

							case "cond.":

								int output3 = return_res (ref t1_has_conditioner);

								if (output3 < 0) {
									resError (returnErrMsg);
								}

								break;

							case "dryer":

								int output4 = return_res (ref t1_has_dryer);

								if (output4 < 0) {
									resError (returnErrMsg);
								}

								break;

							case "sciss":

								int output5 = return_res (ref t1_has_scissors);

								if (output5 < 0) {
									resError (returnErrMsg);
								}

								break;

							case "shamp":

								int output6 = return_res (ref t1_has_shampoo);

								if (output6 < 0) {
									resError (returnErrMsg);
								}

								break;

							case "stati":

								int output7 = return_res (ref t1_has_station);

								if (output7 < 0) {
									resError (returnErrMsg);
								}

								break;

							case "towel":

								int output8 = return_res (ref t1_has_towel);

								if (output8 < 0) {
									resError (returnErrMsg);
								}

								break;
						}

					} else if (b1[t1_curr_index].Substring (0, 3) == "cut") {

						if (!t1_has_brush || !t1_has_scissors) {

							String actionText = b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text;
							b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text = "<color=red>" + actionText + "</color>";
							b3[t1_curr_index].transform.SetParent (layoutPanel.transform);
							b3[t1_curr_index].transform.localScale = Vector3.one;

							resError ("> ERROR: You can't cut without a brush and some scissors.");
							scrollToBottom ();

						} else {

							// perform cut
							t1_did_cut = true;
						}

					} else if (b1[t1_curr_index].Substring (0, 3) == "dry") {

						if (!t1_has_station || !t1_has_dryer || !t1_has_towel) {

							String actionText = b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text;
							b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text = "<color=red>" + actionText + "</color>";
							b3[t1_curr_index].transform.SetParent (layoutPanel.transform);
							b3[t1_curr_index].transform.localScale = Vector3.one;

							resError ("> ERROR: You can't dry without a station, a dryer and a towel.");
							scrollToBottom ();

						} else {

							// perform dry
							t1_did_dry = true;
						}

					} else if (b1[t1_curr_index].Substring (0, 4) == "wash") {

						if (!t1_has_station || !t1_has_shampoo || !t1_has_towel || !t1_has_conditioner) {

							String actionText = b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text;
							b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text = "<color=red>" + actionText + "</color>";
							b3[t1_curr_index].transform.SetParent (layoutPanel.transform);
							b3[t1_curr_index].transform.localScale = Vector3.one;

							resError ("> ERROR: You can't wash without a station, shampoo, conditioner, and a towel.");
							scrollToBottom ();

						} else {

							// perform wash
							t1_did_wash = true;
						}

					} else if (b1[t1_curr_index].Substring (0, 5) == "groom") {

						if (!t1_has_brush || !t1_has_clippers) {

							String actionText = b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text;
							b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text = "<color=red>" + actionText + "</color>";
							b3[t1_curr_index].transform.SetParent (layoutPanel.transform);
							b3[t1_curr_index].transform.localScale = Vector3.one;

							resError ("> ERROR: You can't groom without a brush and some nail clippers.");
							scrollToBottom ();

						} else {

							// perform groom
							t1_did_groom = true;
						}

					} else if (b1[t1_curr_index].Substring (0, 7) == "checkin") {

						if (t1_checkedin) {

							String actionText = b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text;
							b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text = "<color=red>" + actionText + "</color>";
							b3[t1_curr_index].transform.SetParent (layoutPanel.transform);
							b3[t1_curr_index].transform.localScale = Vector3.one;

							resError ("> ERROR: You are already checked in. You have to check out before attempting to check in a different customer.");
							scrollToBottom ();

						} else {

							// perform check-in
							t1_checkedin = true;
							t1_checkedout = false;
						}

					} else if (b1[t1_curr_index].Substring (0, 8) == "checkout") {

						if ((t1_needs_cut && !t1_did_cut) || (t1_needs_dry && !t1_did_dry) ||
							(t1_needs_groom && !t1_did_groom) || (t1_needs_wash && !t1_did_wash)) {

							String actionText = b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text;
							b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text = "<color=red>" + actionText + "</color>";
							b3[t1_curr_index].transform.SetParent (layoutPanel.transform);
							b3[t1_curr_index].transform.localScale = Vector3.one;

							resError ("> ERROR: Seems like you didn't fulfill all of your customer's requests. Please try again.");
							scrollToBottom ();

						} else if (t1_checkedout) {

							String actionText = b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text;
							b3[t1_curr_index].transform.Find ("ActionText").GetComponent<Text> ().text = "<color=red>" + actionText + "</color>";
							b3[t1_curr_index].transform.SetParent (layoutPanel.transform);
							b3[t1_curr_index].transform.localScale = Vector3.one;

							resError ("> ERROR: You have to check in before attempting to check out a customer.");
							scrollToBottom ();

						} else {

							// perform check-out
							t1_checkedin = false;
							t1_checkedout = true;
						}
					}

				} catch { }

				try {

					if (t1_canPrint) {

						if (!err) {
							b3[t1_curr_index].transform.SetParent (layoutPanel.transform);
							b3[t1_curr_index].transform.localScale = Vector3.one;
						}

						t1_curr_index++;
					}
					scrollToBottom ();

				} catch {
					scrollToBottom ();
				}

				j++; // increment step
				scrollToBottom ();
				yield return new WaitForSeconds (1);
			}
		}

		if (!lost) {
			LogManager.instance.logger.sendChronologicalLogs("Level01Won", "", LogManager.instance.UniEndTime().ToString());
			manager.gameWon ();
			Debug.Log ("Finished in " + j + " steps.");

			//logging
			GameLogData.isLevelCleared = true;
			//	LogData.levelSteps = j;
			GameLogData.levelClearedTime = LogManager.instance.EndTimer ();
			GameLogData.levelClearAmount = bar.currentAmount;
			GameLogData.failedReason = "Passed";
			GameLogData.failedAttempts = LogManager.instance.failCount;
			GameLogData.infoButtonCount = LogManager.instance.infoCount;
			GameLogData.agendaButtonCount = LogManager.instance.agendaCount;

			LogManager.instance.CreateLogData ();

			LogManager.instance.isQuitLogNeed = false;

		}

	}

	int acquire (ref bool resource) {

		if (resource) {

			err = true;
			lost = true;
			stop = true;
			paused = true;

			return -1;

		} else {

			resource = true;
			return 0;
		}

	}

	int return_res (ref bool resource) {

		if (!resource) {

			err = true;
			lost = true;
			stop = true;
			paused = true;

			return -1;

		} else {
			resource = false;

			return 0;
		}

	}

	void resError (String msg) {

		GameObject newItem = Instantiate (simulationErrorPrefab) as GameObject;
		newItem.transform.Find ("ActionText").GetComponent<Text> ().text = "<color=red>" + msg + "</color>";
		// newItem.transform.parent = layoutPanel.transform;
		newItem.transform.SetParent (layoutPanel.transform);
		newItem.transform.localScale = Vector3.one;
		scrollToBottom ();

		// terminate simulation
		terminateSimulation (msg);

		scrollToBottom ();

	}

	void clearVerticalLayout () {

		stepsIndicator.text = "0";

		foreach (Transform child in layoutPanel.transform) {
			GameObject.Destroy (child.gameObject);
		}
	}

	void scrollToBottom () {

		//	Debug.Log ("scrollToBottom()");
		simulationScrollRect.verticalNormalizedPosition = 0f;
		Canvas.ForceUpdateCanvases ();
	}

	void clearAllClones () {

		GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject> ();

		foreach (GameObject obj in allObjects) {

			if (obj.transform.name == "SimulationImage(Clone)")
				GameObject.Destroy (obj);
		}
	}

}