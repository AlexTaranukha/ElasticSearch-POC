# importing the requests library 
import requests 
import simplejson

# defining the api-endpoint 
ECEUrl = "http://localhost:9200/audit/tweet/"

for n in range (3000):

    # prepare data 
    b = '{"ID": 66198,"TimeStamp": "2019-07-11T18:48:53.107","ArtifactName": "System","ActionName": "Query","ActionID": 29,"ObjectTypeName": "System","ObjectTypeID": 1,"ExecutionTime": 0,"ArtifactID": 62,"UserName": "Service Account, Relativity","UserID": 777,"Details": {"auditElement": {"QueryText": "' + str(n) + ' '
    a = '\\r\\nSELECT TOP 10000\\r\\n\\t[DataGridAuditFieldMapping].[ArtifactID]\\\\n\\r\\nFROM\\r\\n\\t[DataGridAuditFieldMapping] (NOLOCK\\r\\nLEFT JOIN [ExtendedArtifact] (NOLOCK) ON \\r\\n\\t[ExtendedArtifact].[ArtifactID] = [DataGridAuditFieldMapping].[ArtifactID]\\r\\nWHERE\\\\n([ExtendedArtifact].[AccessControlListID] IS NOT NULL \\r\\nAND \\r\\n[ExtendedArtifact].[AccessControlListID] IN (1))\\r\\nORDER BY \\r\\n\\t[DataGridAuditFieldMapping].[ArtifactID] ASC\\r\\n'
    c = '","QueryParameters": null,"Milliseconds": "0"}},"WorkspaceId": -1,"WorkspaceName": "Admin Case"}'   

    s = ''
    for i in range(n + 1): # 1600000 - 1.35GB, 200000 - 1.68GB
        s += str(a)

    # write inot a file
    f = open('./Data/audit' + str(n) + '.json','w')
    

    f.write(str(b + s + c))

    f.close()

    #post to ECE Cluster

    import os

    myCmd = 'curl -s -H "Content-Type: application/json"  -XPOST ''http://localhost:9200/audit/tweet/' + str(n) + ' --data-binary @./Data/audit' + str(n) + '.json' + ' >/dev/null'
    os.system(myCmd)

    # sending post request and saving response as response object 
    #headers = {'Accept' : 'application/json', 'Content-Type' : 'application/json'}
    #print (ECEUrl + str(n))
    #r = requests.post(url = (ECEUrl + str(n)), headers = headers, data = simplejson.loads(str(b + s + c))) 
    #print (r)