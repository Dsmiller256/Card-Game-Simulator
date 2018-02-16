﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class NetPlayer : NetworkBehaviour
{
    public const string DeckLoadPrompt = "Would you like to join the game with your own deck?";

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("CGSNet: Starting local player");
        LocalNetManager.Instance.LocalPlayer = this;
        if (!isServer)
            CardGameManager.Instance.Messenger.Ask(DeckLoadPrompt, RequestHand, LocalNetManager.Instance.playController.ShowDeckMenu);
    }

    public void RequestHand()
    {
        Debug.Log("CGSNet: Local player requesting hand");
        CmdDealHand();
    }

    [Command]
    public void CmdDealHand()
    {
        Debug.Log("CGSNet: Server received request for hand");
        List<Card> cards = LocalNetManager.Instance.playController.PopDeckCards(CardGameManager.Current.GameStartHandCount);
        TargetDealCards(connectionToClient, cards.Select(card => card.Id).ToArray());
    }

    [TargetRpc]
    public void TargetDealCards(NetworkConnection target, string[] cardIds)
    {
        Debug.Log("CGSNet: Local player received hand");
        List<Card> cards = cardIds.Select(cardId => CardGameManager.Current.Cards[cardId]).ToList();
        LocalNetManager.Instance.playController.AddCardsToHand(cards);
    }

    public void MoveCardToServer(CardStack cardStack, CardModel cardModel)
    {
        Debug.Log("CGSNet: Local player moving card to server");
        cardModel.transform.SetParent(cardStack.transform);
        cardModel.LocalPosition = cardModel.transform.localPosition;
        cardModel.Rotation = cardModel.transform.rotation;
        CmdSpawnCard(cardModel.Id, cardModel.LocalPosition, cardModel.Rotation, cardModel.IsFacedown);
        Destroy(cardModel.gameObject);
    }

    [Command]
    public void CmdSpawnCard(string cardId, Vector3 localPosition, Quaternion rotation, bool isFacedown)
    {
        Debug.Log("CGSNet: Server received card from player");
        PlayMode controller = LocalNetManager.Instance.playController;
        GameObject newCard = Instantiate(LocalNetManager.Instance.cardModelPrefab, controller.playAreaContent);
        CardModel cardModel = newCard.GetComponent<CardModel>();
        cardModel.Value = CardGameManager.Current.Cards[cardId];
        cardModel.transform.localPosition = localPosition;
        cardModel.LocalPosition = localPosition;
        cardModel.transform.rotation = rotation;
        cardModel.Rotation = rotation;
        cardModel.IsFacedown = isFacedown;
        controller.SetPlayActions(controller.playAreaContent.GetComponent<CardStack>(), cardModel);
        NetworkServer.SpawnWithClientAuthority(newCard, gameObject);
        cardModel.RpcHideHighlight();
    }
}
