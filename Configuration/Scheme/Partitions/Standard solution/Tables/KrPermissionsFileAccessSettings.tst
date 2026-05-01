<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="d1b372f3-7565-4309-9037-5e5a0969d94e" ID="95a74318-2e98-46bd-bced-1890dd1cd017" Name="KrPermissionsFileAccessSettings" Group="Kr">
	<SchemePhysicalColumn ID="7e9d555d-de8f-4b5c-9ad2-bc80ed062c12" Name="ID" Type="Int32 Not Null" />
	<SchemePhysicalColumn ID="433c1052-0a35-47e3-b24a-44d08200cffb" Name="Name" Type="String(Max) Not Null" />
	<SchemePrimaryKey ID="fdd637ab-7160-4f7b-9f42-70e66aabcc38" Name="pk_KrPermissionsFileAccessSettings">
		<SchemeIndexedColumn Column="7e9d555d-de8f-4b5c-9ad2-bc80ed062c12" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="7e9d555d-de8f-4b5c-9ad2-bc80ed062c12">0</ID>
		<Name ID="433c1052-0a35-47e3-b24a-44d08200cffb">$KrPermissions_FileAccessSettings_FileNotAvailable</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="7e9d555d-de8f-4b5c-9ad2-bc80ed062c12">1</ID>
		<Name ID="433c1052-0a35-47e3-b24a-44d08200cffb">$KrPermissions_FileAccessSettings_ContentNotAvailable</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="7e9d555d-de8f-4b5c-9ad2-bc80ed062c12">2</ID>
		<Name ID="433c1052-0a35-47e3-b24a-44d08200cffb">$KrPermissions_FileAccessSettings_OnlyLastVersion</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="7e9d555d-de8f-4b5c-9ad2-bc80ed062c12">3</ID>
		<Name ID="433c1052-0a35-47e3-b24a-44d08200cffb">$KrPermissions_FileAccessSettings_OnlyLastAndOwnVersions</Name>
	</SchemeRecord>
</SchemeTable>