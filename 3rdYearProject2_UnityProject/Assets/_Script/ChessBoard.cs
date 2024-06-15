using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChessBoard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color hoverColor;//Color on mouseover
    public Color clickColor;//Click color

    private Color originalColor;//Original
    private bool isClicked = false;
    //private bool HeadLittleBallActivated = false;

    BoardCheck boardCheck;
    PlayerPickUpDrop playerPickUpDrop;

    private void Start()
    {
        originalColor=GetComponent<Renderer>().material.color;//get the original color.
        boardCheck = GameObject.Find("board").GetComponent<BoardCheck>();
        //playerPickUpDrop = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerPickUpDrop>();
    }
    private void Update()
    {
        try
        {
            if (!playerPickUpDrop.GetIsGrabbing())
            {
                if (!isClicked)
                {
                    ChangeColor(originalColor);//when mouseover, change color.
                }
            }
        }
        catch(Exception)
        {
            playerPickUpDrop = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerPickUpDrop>();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playerPickUpDrop.GetIsGrabbing())
        {
            if (!isClicked)
            {
                ChangeColor(hoverColor);//when mouseover, change color.
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isClicked)
        {
            ChangeColor(originalColor);//when mouse move over, change color back.
        }
    }
    /*
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isClicked)
        {
            SetIsClicked(true);
            ChangeColor(clickColor);
        }
    }
    */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("LittleBall"))
        {
            if (!isClicked)
            {
                Vector2 ballLocalPos =new Vector2( this.transform.InverseTransformPoint(collision.gameObject.transform.position).x, this.transform.InverseTransformPoint(collision.gameObject.transform.position).z);
                //Debug.Log("LC "+ballLocalPos);
                if (MathF.Abs(ballLocalPos.x)<0.5&&MathF.Abs(ballLocalPos.y)<0.5)
                {
                    Destroy(collision.gameObject);
                    SetIsClicked(true);
                    ChangeColor(clickColor);
                }
            }
        }
        if (collision.gameObject.CompareTag("BigBall"))
        {
            if (!isClicked)
            {
                Vector2 ballLocalPos = new Vector2(this.transform.InverseTransformPoint(collision.gameObject.transform.position).x, this.transform.InverseTransformPoint(collision.gameObject.transform.position).z);
                //Debug.Log("LC " + ballLocalPos);
                if (MathF.Abs(ballLocalPos.x) < 0.5 && MathF.Abs(ballLocalPos.y) < 0.5)
                {
                    if (ballLocalPos.x < 0 && ballLocalPos.y >= 0)
                    {
                        Destroy(collision.gameObject);
                        boardCheck.BigAddToLine(this.gameObject, 0);

                    }
                    else if (ballLocalPos.x >= 0 && ballLocalPos.y >= 0)
                    {
                        Destroy(collision.gameObject);
                        boardCheck.BigAddToLine(this.gameObject, 1);

                    }
                    else if (ballLocalPos.x < 0 && ballLocalPos.y < 0)
                    {
                        Destroy(collision.gameObject);
                        boardCheck.BigAddToLine(this.gameObject, 2);

                    }
                    else if (ballLocalPos.x >= 0 && ballLocalPos.y < 0)
                    {
                        Destroy(collision.gameObject);
                        boardCheck.BigAddToLine(this.gameObject, 3);

                    }
                }
            }
        }
    }

    public void ChangeColor(Color color)
    {
        if (!isClicked)
            GetComponent<Renderer>().material.color = color;//change cube color.
        else
            GetComponent<Renderer>().material.color = clickColor;
    }

    //-----------------
    public bool GetIsClicked()
    {
        return isClicked;
    }
    public void SetIsClicked(bool value)
    {
        if (value)
        {
            if (!isClicked)
            {
                isClicked = value;
                boardCheck.AddToLine(this.gameObject);
                GetComponent<Renderer>().material.color = clickColor;
            }
        }
        else
        {
            isClicked = value;
            ChangeColor(originalColor);
        }
    }
    //-----------------
}
