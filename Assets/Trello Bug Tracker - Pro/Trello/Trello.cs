using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace DG.TrelloAPI
{
    public class Trello
    {

        private string token;
        private string key;
        private List<object> boards;
        private List<object> lists;
        private List<object> cards;
        private const string memberBaseUrl = "https://api.trello.com/1/members/me";
        private const string boardBaseUrl = "https://api.trello.com/1/boards/";
        private const string listBaseUrl = "https://api.trello.com/1/lists/";
        private const string cardBaseUrl = "https://api.trello.com/1/cards/";
        private string currentBoardId = "";
        //private string currentListId = "";

        // Dictionary<ListName, listId>
        private Dictionary<string, string> cachedLists = new Dictionary<string, string>();

        public Trello(string key, string token)
        {
            this.key = key;
            this.token = token;
        }

        /// <summary>
        /// Checks if a WWW objects has resulted in an error, and if so throws an exception to deal with it.
        /// </summary>
        /// <param name="errorMessage">Error message.</param>
        /// <param name="www">The request object.</param>
        private void CheckWwwStatus(string errorMessage, WWW www)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                throw new TrelloException(errorMessage + ": " + www.error);
            }
        }

        /// <summary>
        /// Checks if the Trello list has already been cached;
        /// </summary>
        /// <param name="listName">The name of the list to check.</param>
        /// <returns>true if the list has been cached </returns>
        public bool IsListCached(string listName)
        {
            return cachedLists.ContainsKey(listName);
        }

        /// <summary>
        /// Async Download a parsed JSON list of the boards in the users account, these are cached on "boards"
        /// </summary>
        /// <returns>  </returns>
        public IEnumerator PopulateBoardsRoutine()
        {
            boards = null;
            WWW www = new WWW(memberBaseUrl + "?" + "key=" + key + "&token=" + token + "&boards=all");

            yield return www;
            CheckWwwStatus("Connection to the Trello servers was not possible", www);

            var dict = Json.Deserialize(www.text) as Dictionary<string, object>;

            boards = (List<object>)dict["boards"];
        }

        /// <summary>
        /// Looks for the board in "boards" and if found sets it as the current board.
        /// </summary>
        /// <param name="name">Name of the board to search.</param>
        public void SetCurrentBoard(string name)
        {
            if (boards == null)
            {
                throw new TrelloException("You have not yet populated the list of boards, so one cannot be selected.");
            }

            for (int i = 0; i < boards.Count; i++)
            {
                var board = (Dictionary<string, object>)boards[i];
                if ((string)board["name"] == name)
                {
                    currentBoardId = (string)board["id"];
                    return;
                }
            }

            currentBoardId = "";
            throw new TrelloException("No such board found.");
        }

        /// <summary>
        /// Async Populate the lists owned by the current board, these are cached for faster card uploading later.
        /// Trello Lists come from trello as a parsed JSON list of lists
        /// </summary>
        /// <returns>.</returns>
        public IEnumerator PopulateListsRoutine()
        {
            lists = null;
            if (currentBoardId == "")
            {
                throw new TrelloException("Cannot retreive the lists, you have not selected a board yet.");
            }

            WWW www = new WWW(boardBaseUrl + currentBoardId + "?" + "key=" + key + "&token=" + token + "&lists=all");

            yield return www;
            CheckWwwStatus("Connection to the Trello servers was not possible", www);

            var dict = Json.Deserialize(www.text) as Dictionary<string, object>;

            lists = (List<object>)dict["lists"];

            // cache the lists
            for (int i = 0; i < lists.Count; i++)
            {
                var list = (Dictionary<string, object>)lists[i];
                if (IsListCached((string)list["name"])) continue;
                cachedLists.Add((string)list["name"], (string)list["id"]);
            }
        }

        /// <summary>
        /// Async Populate the cards for the current list, these are cached for easy card attachment uploading later.
        /// Gets from trello a parsed JSON list of cards.
        /// </summary>
        /// <returns></returns>
        public IEnumerator PopulateCardsFromListRoutine(string listId)
        {
            cards = null;
            if (listId == "")
            {
                throw new TrelloException("Cannot retreive the cards, you have not selected a list yet.");
            }

            WWW www = new WWW(listBaseUrl + listId + "?" + "key=" + key + "&token=" + token + "&cards=all");

            yield return www;
            CheckWwwStatus("Something went wrong: ", www);

            var dict = Json.Deserialize(www.text) as Dictionary<string, object>;
            cards = (List<object>)dict["cards"];
        }

        /// <summary>
        /// Makes a new Trello card object.
        /// </summary>
        /// <returns>The card object.</returns>
        /// <param name="listName">Name of the trello list to which the card will belong.</param>
        public TrelloCard NewCard(string listName)
        {
            string currentListId = "";

            if (IsListCached(listName))
            {
                currentListId = cachedLists[listName];
            }
            else
            {
                throw new TrelloException("List specified not found.");
            }

            var card = new TrelloCard();
            card.idList = currentListId;
            return card;
        }

        /// <summary>
        /// Makes a new Trello card object.
        /// </summary>
        /// <returns>The card object.</returns>
        /// <param name="title">Name of the card.</param>
        /// <param name="description">Description of the card.</param>
        /// <param name="listName">Name of the trello list to which the card will belong.</param>
        /// <param name="newCardsOnTop">Should the card be placed on top of the List?</param>
        public TrelloCard NewCard(string title, string description, string listName, bool newCardsOnTop = true)
        {
            var card = NewCard(listName);
            card.name = title;
            card.desc = description;
            if (newCardsOnTop) card.pos = "top";
            return card;
        }

        /// <summary>
        ///  Creates a new Trello list object, with the current board id.
        ///  It does not upload the list.
        /// </summary>
        /// <returns>The list object.</returns>
        public TrelloList NewList()
        {
            if (currentBoardId == "")
            {
                throw new TrelloException("Cannot create a list if there is no board selected.");
            }

            var list = new TrelloList();
            list.idBoard = currentBoardId;
            return list;
        }

        /// <summary>
        /// Creates a new Trello list object, with the current board id.
        /// </summary>
        /// <returns>The list object.</returns>
        /// <param name="name">Name of the list.</param>
        public TrelloList NewList(string name)
        {
            var list = NewList();
            list.name = name;
            return list;
        }

        /// <summary>
        /// Given an exception object, a TrelloCard is created and populated with the relevant information from the exception. This is then uploaded to the Trello server.
        /// </summary>
        /// <returns>The exception card.</returns>
        /// <param name="e">E.</param>
        //public TrelloCard uploadExceptionCard(Exception e)
        //{
        //    var card = newCard();
        //    card.name = e.GetType().ToString();
        //    card.desc = e.Message;
        //    return uploadCard(card);
        //}

        /// <summary>
        ///  Async uploads a given TrelloCard object to the Trello servers.
        /// </summary>
        /// <returns>Your card ID.</returns>
        /// <param name="card">the card to upload.</param>
        public IEnumerator UploadCardRoutine(TrelloCard card)
        {
            WWWForm post = new WWWForm();
            post.AddField("name", card.name);
            post.AddField("desc", card.desc);
            post.AddField("pos", card.pos);
            post.AddField("due", card.due);
            post.AddField("idList", card.idList);

            WWW www = new WWW(cardBaseUrl + "?" + "key=" + key + "&token=" + token, post);
            yield return www;
            CheckWwwStatus("Could not upload new card to Trello", www);

            var dict = Json.Deserialize(www.text) as Dictionary<string, object>;

            yield return (string)dict["id"];
        }

        /// <summary>
        ///  Async uploads a given TrelloList object to the currently selected board.
        /// </summary>
        /// <returns>Your list ID.</returns>
        /// <param name="list">the list to upload.</param>
        public IEnumerator UploadListRoutine(TrelloList list)
        {
            WWWForm post = new WWWForm();
            post.AddField("name", list.name);
            post.AddField("idBoard", list.idBoard);
            post.AddField("pos", list.pos);

            WWW www = new WWW(listBaseUrl + "?" + "key=" + key + "&token=" + token, post);
            yield return www;
            CheckWwwStatus("Could not upload the new list to Trello", www);

            var dict = Json.Deserialize(www.text) as Dictionary<string, object>;

            yield return (string)dict["id"];
        }

        /// <summary>
        ///  Async uploads an attachment to a given TrelloCard object in the Trello servers.
        /// </summary>
        /// <param name="cardId">Your cards ID.</param>
        /// <param name="attachmentName">The name of the attachment.</param>
        /// <param name="image">A 2d texture for the attachment.</param>
        public IEnumerator SetUpAttachmentInCardRoutine(string cardId, string attachmentName, Texture2D image)
        {
            // Encode texture into PNG
            byte[] bytes = image.EncodeToPNG();

            // Create a Web Form
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", bytes, attachmentName, "image/png");

            WWW www = new WWW(cardBaseUrl + cardId + "/attachments" + "?" + "key=" + key + "&token=" + token, form);
            yield return www;
            CheckWwwStatus("Could not upload the attachment to the card", www);
        }

        /// <summary>
        ///  Async uploads an attachment to a given TrelloCard object on the Trello servers.
        /// </summary>
        /// <param name="cardId">the cards ID.</param>
        /// <param name="attachmentName">The name of the attachment.</param>
        /// <param name="data">Any aditional data needed i.e save file </param>
        public IEnumerator SetUpAttachmentInCardRoutine(string cardId, string attachmentName, string data)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data.ToCharArray());
            //byte[] bytes = new byte[data.Length * sizeof(char)];
            //System.Buffer.BlockCopy(data.ToCharArray(), 0, bytes, 0, bytes.Length);

            // Create a Web Form
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", bytes, attachmentName, "text/plain");
            
            WWW www = new WWW(cardBaseUrl + cardId + "/attachments" + "?" + "key=" + key + "&token=" + token, form);
            
            yield return www;
            CheckWwwStatus("Could not upload the attachment to the card", www);
        }

        /// <summary>
        ///  Async uploads an attachment to a given TrelloCard object in the Trello servers.
        /// </summary>
        /// <param name="cardId">the cards ID.</param>
        /// <param name="attachmentName">The name of the attachment.</param>
        /// <param name="path">the path to the text file to be attached</param>
        public IEnumerator SetUpAttachmentInCardFromFileRoutine(string cardId, string attachmentName, string path)
        {
            Debug.Assert(System.IO.File.Exists(path), "The path to the log file specified is not correct");

            byte[] bytes = System.IO.File.ReadAllBytes(path);

            // Create a Web Form
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", bytes, attachmentName, "text/plain");

            WWW www = new WWW(cardBaseUrl + cardId + "/attachments" + "?" + "key=" + key + "&token=" + token, form);
            yield return www;
            CheckWwwStatus("Could not upload the attachment to the card", www);
        }

        /// <summary>
        /// Async Populate the cards for the current list, these are cached for easy card attachment uploading later.
        /// </summary>
        /// <returns>A parsed JSON list of cards.</returns>
        ///
        public bool IsConnected()
        {
            return (currentBoardId != "") ? true : false;
        }
    }
}