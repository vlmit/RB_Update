<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="d1b372f3-7565-4309-9037-5e5a0969d94e" ID="937fdcfd-c412-4b5d-a319-c11684ea009a" Name="KrPermissionsSystem" Group="Kr">
	<Description>Системная таблица для правил доступа.</Description>
	<SchemePhysicalColumn ID="98a3e6b5-099f-463f-bf96-92817ed362ad" Name="Version" Type="Int64 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="c6b2598a-ca10-45ce-be39-98eeb0495ebc" Name="df_KrPermissionsSystem_Version" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="0d52e7ea-dfa0-4cb7-8310-a61c9a532805" Name="Readers" Type="Int32 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="35c89c3c-3141-459d-af68-91f9fd87c788" Name="df_KrPermissionsSystem_Readers" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="6b510386-3d91-40d2-b4e6-e726eeef89f9" Name="Writers" Type="Int32 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="a4dac62a-9950-49d3-91b2-46309e0ce276" Name="df_KrPermissionsSystem_Writers" Value="0" />
	</SchemePhysicalColumn>
</SchemeTable>