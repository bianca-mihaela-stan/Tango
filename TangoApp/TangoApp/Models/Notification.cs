using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace TangoApp.Models
{
    public class Notification
    {    
        //identificam notificarea pe baza unui int unic
        [Key]
        public int Id { get; set; }
        // userId = id-ul userului care o sa primeasca notificarea
        public DateTime Time { get; set; }
        // Bazandu-ne pe modelul notificariilor de la Facebook, o sa ne luam inca o variabila booleana Seen:
        //daca Seen = True , inseamna ca userul a dat click pe notificare
        //si a fost redirectionat la pagina corespunzatoare
        //altfel, o afisam stilizat (notificarea nu a fost vazuta)
        public bool Seen { get; set; }
        //in cazul in care nu luam in evidenta comentariile in care un user poate 
        //sa mentioneze alt user - diferit de persoana care a lasat comentariul - 
        // atunci UserReceiveId = Post.UserId
        //altfel, mai avem nevoie de un field care sa referentieze UserId din ApplicationUser:
        public string UserReceiveId { get; set; }
        [ForeignKey("UserReceiveId")]
        public virtual ApplicationUser UserReceive { get; set; }

   
        public string UserSendId { get; set; }
        [ForeignKey("UserSendId")]
        public virtual ApplicationUser UserSend { get; set; }
        // postId = id-ul postarii pe baza careia se creeaza notificarea
        // Daca administratorul a sters o postare sau un comentariu de la o postare, id-ul postarii respective
        // va ocupa acest field
        public int? PostId { get; set; }
        public virtual Post Post { get; set; }
        //Daca administratorul sterge un comentariu de la o postare, nu e indeajuns sa retinem doar id-ul postarii
        // respective => trebuie sa retinem si id-ul comentariului sters, pe care o sa il afisam userului
        //la notificari: Ex. Comentariul urmator la postarea X a fost gasit neadecvat si prin urmare sters: 
        // [continutul comentariului]
        public int? CommentId { get; set; }
        public virtual Comment Comment { get; set; }
        //Cum ne dam seama de tipul notificarii: 
        //daca administratorul sterge o postare => campul cu commentId o sa fie null (continutul sters a fost 
        //de tip "postare")
        //daca administratorul sterge un comentariu de la o postare => campurile commentId si postId o sa fie
        // ambele completate (continutul sters a fost de tip "comentariu")
        public NotificationFlag Type { get; set; }
    }
    public enum NotificationFlag
    {
        DeletedComment,
        DeletedPost,
        NewComment,
        NewPost
    };
}