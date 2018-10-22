﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public int health;

    private Rhythm rhythmController;
    private StateController controller;
    private EnemyBoundaryChecker boundaryChecker;
    private GameObject enemy;
    private GameObject player;
    private bool isBerserk;
    private bool isDead;

    public Animator anim;

	// Use this for initialization
	void Start () {
        rhythmController = GameObject.Find("Rhythm").GetComponent<Rhythm>();
        rhythmController.onEnemyBeat += doEnemyAction;

        controller = this.gameObject.GetComponent<StateController>();

        boundaryChecker = new EnemyBoundaryChecker();

        enemy = this.gameObject.transform.parent.gameObject;
        player = GameObject.Find("Player").gameObject;

        isBerserk = false;
        isDead = false;

        anim.SetBool("B_Died", false);
    }

    private void doEnemyAction() {
        anim.SetBool("B_Left", false);
        anim.SetBool("B_Right", false);
        anim.SetBool("B_Hit", false);
        anim.SetBool("B_Atk", false);
        if (controller != null) {
            controller.UpdateState();
        }
    }

    public void takeDamage(int dmg) {
        if (!isDead) {
            handleDamageTaken(dmg);
        }
    }

    private void handleDamageTaken(int dmg) {
        dmg = (int) (rhythmController.playerComboCount * 0.1 + dmg);

        Debug.Log("enemy damage taken");
        anim.SetBool("B_Hit", true);
        if (health - dmg >= 0) {
            health -= dmg;
        }
        else {
            anim.SetBool("B_Died", true);
            health = 0;
        }
        EventManager.TriggerEvent("ReduceEnemyHP");
        checkAndDestroyIfDead();
        checkAndSetEnemyBerserk();
    }

    private void checkAndSetEnemyBerserk() {
        if (health < health / 2) {
            isBerserk = true;
        }
    }

    private void checkAndDestroyIfDead() {
        if (health <= 0) {
            Debug.Log("dead");
            //Create a delay here for the death animation
            StartCoroutine(doDeathAnimationAndDestroyObject(4.0f));
        }
    }

    private IEnumerator doDeathAnimationAndDestroyObject(float duration) {
        float startTime = Time.time;
        isDead = true;
        controller = null;
        while (duration >= (Time.time - startTime)) {
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(this.gameObject.transform.parent.gameObject);
    }

    public bool decideMoveOrCharge() {
        int value = Random.Range(0, 10);
        if (value < 7) {
            return false;
        }
        return true;
    }

    public void doMovementORStayIdle() {
        int rand = Random.Range(0, 2);
        if (rand < 1) {
            stayIdle();
        }
        else {
            doMovement();
        }
    }

    private void stayIdle() {
        // does nothing
        Debug.Log("Enemy idle");
    }

    private void doMovement() {
        Vector3 newPosition;
        int movementNumber = Random.Range(0, 2);

        switch (movementNumber) {
            //left but right if at left boundary
            case 0:
                newPosition = enemy.transform.position + new Vector3(-1, 0, 0);
                if (checkValidMovement(newPosition)) {
                    anim.SetBool("B_Right", true);
                    anim.SetBool("B_Left", false);
                    StartCoroutine(locationTransition(enemy.transform.position, newPosition));
                }
                //temporary code
                else {
                    anim.SetBool("B_Right", false);
                    anim.SetBool("B_Left", true);
                    newPosition = enemy.transform.position + new Vector3(1, 0, 0);
                    StartCoroutine(locationTransition(enemy.transform.position, newPosition));
                }

                break;
            //right but left if at right boundary
            case 1:
                newPosition = enemy.transform.position + new Vector3(1, 0, 0);
                if (checkValidMovement(newPosition)) {
                    anim.SetBool("B_Right", false);
                    anim.SetBool("B_Left", true);
                    StartCoroutine(locationTransition(enemy.transform.position, newPosition));
                }
                //temporary code
                else {
                    newPosition = enemy.transform.position + new Vector3(-1, 0, 0);
                    anim.SetBool("B_Right", true);
                    anim.SetBool("B_Left", false);
                    StartCoroutine(locationTransition(enemy.transform.position, newPosition));
                }
                break;
        }
    }

    IEnumerator locationTransition(Vector3 startPosition, Vector3 endPosition) {
        float currentAnimationTime = 0.0f;
        float totalAnimationTime = 0.05f;
        while (currentAnimationTime < totalAnimationTime) {
            currentAnimationTime += Time.deltaTime;
            enemy.transform.position = Vector3.Lerp(startPosition, endPosition, currentAnimationTime / totalAnimationTime);
            yield return new WaitForSeconds(0.01f);
        }
    }

    private bool checkValidMovement(Vector3 pos) {
        return boundaryChecker.checkValidMovement(pos);
    }

    public bool checkValidGeneralPosition(Vector3 pos) {
        return boundaryChecker.checkValidGeneralPosition(pos);
    }

    public GameObject getPlayer() {
        return player;
    }

    public bool getIsBerserk() {
        return isBerserk;
    }

}
