using Subtegral.DialogueSystem.DataContainers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialoguer : MonoBehaviour
{
    [SerializeField]
    private DialogueContainer container;
    [SerializeField]
    private Text[] answerTexts;
    [SerializeField]
    private Text npcMessage;


    private NodeLinkData currentPort;

    // Start is called before the first frame update
    void Start()
    {
        currentPort = container.NodeLinks[0];
        for (int i = 0; i < answerTexts.Length; i++)
        {
            var btn = answerTexts[i].GetComponentInChildren<Button>();
            var aux = i;
            btn.onClick.AddListener(() => Answer(aux));
        }
        Ask();
    }

    private void Ask()
    {
        //Continuing the conversation by making the NPC ask something again.
        //NOTE: Ask isn't necessarily an question. It's just something that can be replied.
        npcMessage.text = container.DialogueNodeData.Find(x => x.NodeGUID == currentPort.TargetNodeGUID).DialogueText;

        List<NodeLinkData> validPorts = GetAnswerPorts();

        for (int i = 0; i < answerTexts.Length; i++)
        {
            if (validPorts.Count <= i)
            {
                answerTexts[i].text = "";
                answerTexts[i].GetComponentInChildren<Image>().enabled = false;
                continue;
            }
            answerTexts[i].text = validPorts[i].PortName;
        }
    }

    private void Answer(int index)
    {
        //Testing if the chosen port is a segment of the current node.
        List<NodeLinkData> validPorts = GetAnswerPorts();
        if (validPorts.Count <= index)
        {
            //If there are no segments, then that's the end of conversation.
            //There are 2 ways of ending in here: end of conversation, or there's no reply assigned to the index.
            return;
        }
        
        currentPort = validPorts[index];

        Ask();
    }

    private List<NodeLinkData> GetAnswerPorts()
    {
        return container.NodeLinks.Where(x => x.BaseNodeGUID == currentPort.TargetNodeGUID).ToList();
    }

}
