# Linkly.PayAtTable

The Pay at Table API provides a common interface for the PIN pad to utilise the EFT-Client to retrieve available tables and orders so payment functions (e.g. tender, customer receipt etc.) can be performed by an operator on the PIN pad without using the main POS UI. 

The Pay at Table client requires the POS to act a data source so that it can retrieve information about available tables, orders, payment options etc. 

The Pay at Table client supports two data source options for the POS; a REST server or directly through the existing Linkly interface. 

## Start Developing

To start developing the Linkly Pay at Table solution, following the instructions on the [Linkly API](http://linkly.com.au/apidoc/TCPIP/#pay-at-table).

## REST Server 
When in REST server mode the Pay at Table extension will connect directly to the POS REST Server. 

An example REST server can be found in the [PayAtTable.Server](PayAtTable.Server) folder.

![REST INTERFACE DIAGRAM](PayAtTable.Docs/rest-interface.png)

## PC-EFTPOS Interface 
When in POS mode the Pay at Table extension will utilise the existing interface between the POS and EFT-Client (i.e. the interface used to perform a transaction using PC-EFTPOS).  

If the POS has implemented the PC-EFTPOS Active X interface the POS client must reside on the same PC as the EFT-Client, if the POS has implemented the PC-EFTPOS TCP/IP interface the POS client can reside on a different PC. 

An example PC-EFTPOS interface example for both ActiveX and TCP/IP can be found in the [PayAtTable.TestPos](PayAtTable.TestPos) folder.

![POS INTERFACE DIAGRAM](PayAtTable.Docs/pos-interface-local.png)

![POS INTERFACE DIAGRAM](PayAtTable.Docs/pos-interface-remote.png)

