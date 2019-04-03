1. Le prefab SocketIO doit être dans la scène.

2. Mettre le script "CrowdInteraction" sur un objet.

3. Avant de démarrer un sondage: On doit enregistrer une méthode qui sera appelée quand le résultat nous est envoyé. 
Utilisez la méthode public de CrowdInteraction "SetNotifyMethod(NomDeLaMethode)". La définition de la méthode doit avoir l'air de ceci:
public void NomDeLaMethode(SocketIOEvent eventData)

4. Pour commencer un sondage: Utilisez la méthode public de CrowdInteraction "PollCrowd(List<string> options, int duration)".

5. Après la duration du sondage écoulé, vous recevrai le résultat par le paramètre eventData en JSON. Accéder au résultat avec eventData.data["result"]. (Voir exemple dans le script CrowdInteraction)

Vous pouvez voir un exemple dans la scène CrowdInteraction. 