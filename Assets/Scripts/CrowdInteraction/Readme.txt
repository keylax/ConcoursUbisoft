1. Le prefab SocketIO doit �tre dans la sc�ne.

2. Mettre le script "CrowdInteraction" sur un objet.

3. Avant de d�marrer un sondage: On doit enregistrer une m�thode qui sera appel�e quand le r�sultat nous est envoy�. 
Utilisez la m�thode public de CrowdInteraction "SetNotifyMethod(NomDeLaMethode)". La d�finition de la m�thode doit avoir l'air de ceci:
public void NomDeLaMethode(SocketIOEvent eventData)

4. Pour commencer un sondage: Utilisez la m�thode public de CrowdInteraction "PollCrowd(List<string> options, int duration)".

5. Apr�s la duration du sondage �coul�, vous recevrai le r�sultat par le param�tre eventData en JSON. Acc�der au r�sultat avec eventData.data["result"]. (Voir exemple dans le script CrowdInteraction)

Vous pouvez voir un exemple dans la sc�ne CrowdInteraction. 