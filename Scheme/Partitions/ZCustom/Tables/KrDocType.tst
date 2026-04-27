<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="78bfc212-cad5-4d1d-8b91-a9c58562b9d5" Partition="d1b372f3-7565-4309-9037-5e5a0969d94e">
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="7372fec8-80b6-40b9-b31b-238be3b8ef9f" Name="ExternalID" Type="String(128) Null">
		<Description>Поле для указания ID из SP по типу документа</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="41c5987f-cbfb-4c66-9f23-959d5bfba43e" Name="AutoApproveStartingDate" Type="Date Null">
		<Description>Дата создания документов, с которой работает автосогласования</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="54458715-9000-4aa9-8430-432a44de209c" Name="df_KrDocType_AutoApproveStartingDate" />
	</SchemePhysicalColumn>
</SchemeTable>