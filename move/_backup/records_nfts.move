module space_fighters::records_nfts {
    use std::error;
    use std::signer;
    use std::string::{String, utf8};
    use std::option::{Self, Option};

    use aptos_framework::object::{Self, Object};
    use aptos_token_objects::collection;
    use aptos_token_objects::token::{Self, Token};
    
    use space_fighters::composable_nfts;

    /// The account is not authorized to update the resources.
    const ENOT_AUTHORIZED: u64 = 1;
    /// Token is not owned by the user
    const ENOT_OWNER: u64 = 2;

    const PILOT_COLLECTION_NAME: vector<u8> = b"Aptos Space Fighters Pilots";
    const PILOT_COLLECTION_DESCRIPTION: vector<u8> = b"This collection is not tradable! It represents your identity in game and tracks your progress.";
    const PILOT_COLLECTION_URI: vector<u8> = b"https://storage.googleapis.com/space-fighters-assets/nft_pilot.jpg";
    
    const PILOT_TOKEN_NAME: vector<u8> = b"Pilot";
    const PILOT_TOKEN_DESCRIPTION: vector<u8> = b"This is your fighter";
    const PILOT_TOKEN_URI: vector<u8> = b"https://storage.googleapis.com/space-fighters-assets/collection_pilot.jpg";

    const RECORDS_COLLECTION_NAME: vector<u8> = b"Aptos Space Fighter Records";
    const RECORDS_COLLECTION_DESCRIPTION: vector<u8> = b"This collection is not tradable! It provides records of progress in game.";
    const RECORDS_COLLECTION_URI: vector<u8> = b"";
    
    const RECORDS_TOKEN_NAME: vector<u8> = b"Records";
    const RECORDS_TOKEN_DESCRIPTION: vector<u8> = b"";
    const RECORDS_TOKEN_URI: vector<u8> = b"";

    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct CollectionConfig has key {
        mutator_ref: collection::MutatorRef,
    }

    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct TokenMetadata has key {
        /// Used to burn.
        burn_ref: token::BurnRef,
        /// Used to control freeze.
        transfer_ref: object::TransferRef,
        /// Used to mutate fields
        mutator_ref: token::MutatorRef,
    }

    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct Pilot has key {
        records: Object<Records>,
        aptos_name: Option<String>,
        token_v2_avatar: Option<Object<Token>>,
    }

    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct Records has key {
        games_played: u64,
        longest_survival_ms: u64,
    }

    struct PilotDataView has drop {
        aptos_name: Option<String>,
        avatar: Option<String>,
        games_played: u64,
        longest_survival_ms: u64,
    }

    fun init_module(
        admin: &signer,
    ) {
        create_pilot_collection(admin);
        create_records_collection(admin);
    }

    fun create_pilot_collection(
        resource_account: &signer
    ) {
        collection::create_unlimited_collection(
            resource_account,
            utf8(PILOT_COLLECTION_DESCRIPTION),
            utf8(PILOT_COLLECTION_NAME),
            option::none(),
            utf8(PILOT_COLLECTION_URI),
        );
    }

    fun create_records_collection(
        resource_account: &signer
    ) {
        collection::create_unlimited_collection(
            resource_account,
            utf8(RECORDS_COLLECTION_DESCRIPTION),
            utf8(RECORDS_COLLECTION_NAME),
            option::none(),
            utf8(RECORDS_COLLECTION_URI),
        );
    }

    entry fun mint_pilot_and_records(
        admin: &signer,
        mint_to: address,
    ) {
        assert!(signer::address_of(admin) == @space_fighters, error::permission_denied(ENOT_AUTHORIZED));
        let records = mint_records(admin, mint_to);
        mint_pilot(admin, mint_to, records);
    }

    fun mint_pilot(
        admin: &signer,
        mint_to: address,
        records: Object<Records>,
    ): Object<Pilot> {
        let constructor_ref = token::create_from_account(
            admin,
            utf8(PILOT_COLLECTION_NAME),
            utf8(PILOT_TOKEN_DESCRIPTION),
            utf8(PILOT_TOKEN_NAME),
            option::none(),
            utf8(PILOT_TOKEN_URI),
        );
        let object_signer = object::generate_signer(&constructor_ref);
        let transfer_ref = object::generate_transfer_ref(&constructor_ref);
        let mutator_ref = token::generate_mutator_ref(&constructor_ref);
        let burn_ref = token::generate_burn_ref(&constructor_ref);

        // Transfers the token to the `mint_to` address
        composable_nfts::creator_transfer(&transfer_ref, mint_to);
        // Disables ungated transfer, thus making the token soulbound and non-transferable
        object::disable_ungated_transfer(&transfer_ref);

        let token_metadata = TokenMetadata {
            burn_ref,
            transfer_ref,
            mutator_ref,
        };
        move_to(&object_signer, token_metadata);

        let pilot = Pilot {
            records,
            aptos_name: option::none(),
            token_v2_avatar: option::none(),
        };
        move_to(&object_signer, pilot);

        object::object_from_constructor_ref(&constructor_ref)
    }

    fun mint_records(
        admin: &signer,
        mint_to: address,
    ): Object<Records> {
        let constructor_ref = token::create_from_account(
            admin,
            utf8(RECORDS_COLLECTION_NAME),
            utf8(RECORDS_TOKEN_DESCRIPTION),
            utf8(RECORDS_TOKEN_NAME),
            option::none(),
            utf8(RECORDS_TOKEN_URI),
        );
        let object_signer = object::generate_signer(&constructor_ref);
        let transfer_ref = object::generate_transfer_ref(&constructor_ref);
        let mutator_ref = token::generate_mutator_ref(&constructor_ref);
        let burn_ref = token::generate_burn_ref(&constructor_ref);

        // Transfers the token to the `mint_to` address
        composable_nfts::creator_transfer(&transfer_ref, mint_to);
        // Disables ungated transfer, thus making the token soulbound and non-transferable
        object::disable_ungated_transfer(&transfer_ref);

        let token_metadata = TokenMetadata {
            burn_ref,
            transfer_ref,
            mutator_ref,
        };
        move_to(&object_signer, token_metadata);

        let pilot = Records {
            games_played: 0,
            longest_survival_ms: 0,
        };
        move_to(&object_signer, pilot);

        object::object_from_constructor_ref(&constructor_ref)
    }

    entry fun set_pilot_aptos_name(
        admin: &signer,
        owner: address,
        pilot: Object<Pilot>,
        name: Option<String>,
    ) acquires Pilot {
        assert!(signer::address_of(admin) == @space_fighters, error::permission_denied(ENOT_AUTHORIZED));
        assert!(object::is_owner(pilot, owner), error::permission_denied(ENOT_OWNER));

        let pilot_obj = borrow_global_mut<Pilot>(object::object_address(&pilot));
        pilot_obj.aptos_name = name;
    }

    entry fun set_pilot_aptos_avatar_v2(
        admin: &signer,
        owner: address,
        pilot: Object<Pilot>,
        token: Option<Object<Token>>,
    ) acquires Pilot {
        assert!(signer::address_of(admin) == @space_fighters, error::permission_denied(ENOT_AUTHORIZED));
        assert!(object::is_owner(pilot, owner), error::permission_denied(ENOT_OWNER));
        let pilot_obj = borrow_global_mut<Pilot>(object::object_address(&pilot));
        pilot_obj.token_v2_avatar = token;
    }

    entry fun save_game_result(
        admin: &signer,
        owner: address,
        pilot: Object<Pilot>,
        ms_survived: u64,
    ) acquires Pilot, Records {
        assert!(signer::address_of(admin) == @space_fighters, error::permission_denied(ENOT_AUTHORIZED));
        assert!(object::is_owner(pilot, owner), error::permission_denied(ENOT_OWNER));
        let records = &borrow_global<Pilot>(object::object_address(&pilot)).records;
        let records_obj = borrow_global_mut<Records>(object::object_address(records));
        records_obj.games_played = records_obj.games_played + 1;
        if (ms_survived > records_obj.longest_survival_ms) {
            records_obj.longest_survival_ms = ms_survived;
        }
    }

    #[view]
    public fun view_pilot_records(
        pilot: Object<Pilot>,
    ): PilotDataView acquires Pilot, Records {
        let pilot_obj = borrow_global<Pilot>(object::object_address(&pilot));
        let token_v2_avatar = &pilot_obj.token_v2_avatar;
        let avatar = if (option::is_some(token_v2_avatar)) {
            option::some(token::uri(*option::borrow(token_v2_avatar)))
        } else {
            option::none()
        };

        let records_obj = borrow_global_mut<Records>(object::object_address(&pilot_obj.records));
        PilotDataView {
            aptos_name: pilot_obj.aptos_name,
            avatar,
            games_played: records_obj.games_played,
            longest_survival_ms: records_obj.longest_survival_ms,
        }
    }

    #[view]
    public fun view_pilot_records_v2(
        pilot: address,
    ): PilotDataView acquires Pilot, Records {
        let pilot_obj = borrow_global<Pilot>(pilot);
        let token_v2_avatar = &pilot_obj.token_v2_avatar;
        let avatar = if (option::is_some(token_v2_avatar)) {
            option::some(token::uri(*option::borrow(token_v2_avatar)))
        } else {
            option::none()
        };

        let records_obj = borrow_global_mut<Records>(object::object_address(&pilot_obj.records));
        PilotDataView {
            aptos_name: pilot_obj.aptos_name,
            avatar,
            games_played: records_obj.games_played,
            longest_survival_ms: records_obj.longest_survival_ms,
        }
    }

    #[test(admin = @0x123, user = @0x2)]
    public fun test_pilot_records(
        admin: signer,
        user: signer,
    ) acquires Pilot, Records {
        use aptos_framework::account;

        let admin_addr = signer::address_of(&admin);
        let user_addr = signer::address_of(&user);

        account::create_account_for_test(admin_addr);
        account::create_account_for_test(user_addr);

        init_module(&admin);
        let records = mint_records(&admin, user_addr);
        let pilot = mint_pilot(&admin, user_addr, records);
        assert!(&view_pilot_records(pilot) == &PilotDataView {
            aptos_name: option::none(),
            avatar: option::none(),
            games_played: 0,
            longest_survival_ms: 0,
        }, 1);

        set_pilot_aptos_name(&admin, user_addr, pilot, option::some(utf8(b"test.apt")));
        assert!(&view_pilot_records(pilot) == &PilotDataView {
            aptos_name: option::some(utf8(b"test.apt")),
            avatar: option::none(),
            games_played: 0,
            longest_survival_ms: 0,
        }, 1);

        let token = object::address_to_object<Token>(object::object_address(&pilot));
        set_pilot_aptos_avatar_v2(&admin, user_addr, pilot, option::some(token));
        assert!(&view_pilot_records(pilot) == &PilotDataView {
            aptos_name: option::some(utf8(b"test.apt")),
            avatar: option::some(utf8(PILOT_TOKEN_URI)),
            games_played: 0,
            longest_survival_ms: 0,
        }, 1);

        save_game_result(&admin, user_addr, pilot, 5000);
        assert!(&view_pilot_records(pilot) == &PilotDataView {
            aptos_name: option::some(utf8(b"test.apt")),
            avatar: option::some(utf8(PILOT_TOKEN_URI)),
            games_played: 1,
            longest_survival_ms: 5000,
        }, 1);

        save_game_result(&admin, user_addr, pilot, 4000);
        assert!(&view_pilot_records(pilot) == &PilotDataView {
            aptos_name: option::some(utf8(b"test.apt")),
            avatar: option::some(utf8(PILOT_TOKEN_URI)),
            games_played: 2,
            longest_survival_ms: 5000,
        }, 1);

        save_game_result(&admin, user_addr, pilot, 8000);
        assert!(&view_pilot_records(pilot) == &PilotDataView {
            aptos_name: option::some(utf8(b"test.apt")),
            avatar: option::some(utf8(PILOT_TOKEN_URI)),
            games_played: 3,
            longest_survival_ms: 8000,
        }, 1);
    }
}