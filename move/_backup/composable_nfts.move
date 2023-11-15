module space_fighters::composable_nfts {
    use std::error;
    use std::signer;
    use std::string::{String, utf8};
    use std::option::{Self, Option};

    use aptos_framework::object::{Self, Object, TransferRef};
    use aptos_token_objects::collection;
    use aptos_token_objects::token;
    use aptos_token_objects::property_map;
    use aptos_framework::event;

    friend space_fighters::records_nfts;
    friend space_fighters::payments;

    /// The account is not authorized to update the resources.
    const ENOT_AUTHORIZED: u64 = 1;
    /// Token is not owned by the user
    const ENOT_OWNER: u64 = 2;

    const FIGHTER_COLLECTION_NAME: vector<u8> = b"Aptos Space Fighters";
    const FIGHTER_COLLECTION_DESCRIPTION: vector<u8> = b"Play Aptos Space Fighters game and earn rewards!";
    const FIGHTER_COLLECTION_URI: vector<u8> = b"https://storage.googleapis.com/space-fighters-assets/collection_fighter.jpg";
    
    const PARTS_COLLECTION_NAME: vector<u8> = b"Aptos Space Fighter Parts";
    const PARTS_COLLECTION_DESCRIPTION: vector<u8> = b"Play Aptos Space Fighters game and improve your fighters with these parts!";
    const PARTS_COLLECTION_URI: vector<u8> = b"https://storage.googleapis.com/space-fighters-assets/collection_parts.jpg";
    
    const FIGHTER_TYPE: vector<u8> = b"Fighter";
    const WING_TYPE: vector<u8> = b"Wing";
    const BODY_TYPE: vector<u8> = b"Body";
    
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
        /// Used to mutate properties
        property_mutator_ref: property_map::MutatorRef,
        /// Used to emit MintEvent
        mint_events: event::EventHandle<MintEvent>,
    }

    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct Fighter has key {
        body: Option<Object<Body>>,
        wings: Option<Object<Wing>>,
    }

    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct FighterBaseAttributes has key {
        Speed: u64,
        Health: u64,
    }

    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct Body has key {}
    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct Wing has key {}

    #[resource_group_member(group = aptos_framework::object::ObjectGroup)]
    struct Attributes has key, drop {
        Speed: u64,
        Health: u64,
        Type: String,
    }

    struct MintEvent has drop, store {
        token_receiver_address: address,
        token_data_id: address,
        type: String,
        token_uri: String,
        token_name: String,
    }

    fun init_module(admin: &signer) {
        create_fighter_collection(admin);
        create_parts_collection(admin);
    }

    fun create_fighter_collection(
        admin: &signer
    ) {
        collection::create_unlimited_collection(
            admin,
            utf8(FIGHTER_COLLECTION_DESCRIPTION),
            utf8(FIGHTER_COLLECTION_NAME),
            option::none(),
            utf8(FIGHTER_COLLECTION_URI),
        );
    }

    fun create_parts_collection(
        admin: &signer
    ) {
        collection::create_unlimited_collection(
            admin,
            utf8(PARTS_COLLECTION_DESCRIPTION),
            utf8(PARTS_COLLECTION_NAME),
            option::none(),
            utf8(PARTS_COLLECTION_URI),
        );
    }

    entry fun mint_fighter(
        admin: &signer,
        mint_to: address,
        token_name: String,
        token_description: String,
        token_uri: String,
        health: u64,
        speed: u64,
    ) {
        assert!(signer::address_of(admin) == @space_fighters, error::permission_denied(ENOT_AUTHORIZED));
        mint_internal(
            admin,
            mint_to,
            utf8(FIGHTER_COLLECTION_NAME),
            token_name,
            token_description,
            token_uri,
            utf8(FIGHTER_TYPE),
            health,
            speed,
        )
    }

    entry fun mint_wing(
        admin: &signer,
        mint_to: address,
        token_name: String,
        token_description: String,
        token_uri: String,
        health: u64,
        speed: u64,
    ) {
        assert!(signer::address_of(admin) == @space_fighters, error::permission_denied(ENOT_AUTHORIZED));
        mint_internal(
            admin,
            mint_to,
            utf8(PARTS_COLLECTION_NAME),
            token_name,
            token_description,
            token_uri,
            utf8(WING_TYPE),
            health,
            speed,
        )
    }
    entry fun mint_body(
        admin: &signer,
        mint_to: address,
        token_name: String,
        token_description: String,
        token_uri: String,
        health: u64,
        speed: u64,
    ) {
        assert!(signer::address_of(admin) == @space_fighters, error::permission_denied(ENOT_AUTHORIZED));
        mint_internal(
            admin,
            mint_to,
            utf8(PARTS_COLLECTION_NAME),
            token_name,
            token_description,
            token_uri,
            utf8(BODY_TYPE),
            health,
            speed,
        )
    }

    /// Add or swap parts. Remove isn't supported.
    entry fun swap_or_add_parts(
        admin: &signer,
        owner: address,
        fighter: Object<Fighter>,
        wing: Option<Object<Wing>>,
        body: Option<Object<Body>>,
    ) acquires Fighter, Wing, Body, TokenMetadata, FighterBaseAttributes, Attributes {
        assert!(signer::address_of(admin) == @space_fighters, error::permission_denied(ENOT_AUTHORIZED));
        let fighter_obj = borrow_global_mut<Fighter>(object::object_address(&fighter));
        let fighter_address = object::object_address(&fighter);
        // need to make sure that the owner owns the fighter
        assert!(object::is_owner(fighter, owner), error::permission_denied(ENOT_OWNER));

        if (option::is_some(&wing)) {
            // Transfer old wing
            let old_wing = fighter_obj.wings;
            if (option::is_some(&old_wing)) {
                let transfer_ref = &borrow_global<TokenMetadata>(object::object_address(option::borrow(&old_wing))).transfer_ref;
                creator_transfer(transfer_ref, owner);
            };
            let new_wing = *option::borrow(&wing);
            // even if the old wing was the wing we want to switch to, this will still work
            assert!(object::is_owner(new_wing, owner), error::permission_denied(ENOT_OWNER));
            // Ensure that it's a wing!
            borrow_global_mut<Wing>(object::object_address(option::borrow(&wing)));
            let transfer_ref = &borrow_global<TokenMetadata>(object::object_address(option::borrow(&wing))).transfer_ref;
            option::swap_or_fill(&mut fighter_obj.wings, new_wing);
            creator_transfer(transfer_ref, fighter_address);
        };
        if (option::is_some(&body)) {
            let old_body = fighter_obj.body;
            if (option::is_some(&old_body)) {
                let transfer_ref = &borrow_global<TokenMetadata>(object::object_address(option::borrow(&old_body))).transfer_ref;
                creator_transfer(transfer_ref, owner);
            };
            let new_body = *option::borrow(&body);
            assert!(object::is_owner(new_body, owner), error::permission_denied(ENOT_OWNER));
            // Ensure that it's a body!
            borrow_global_mut<Body>(object::object_address(option::borrow(&body)));
            let transfer_ref = &borrow_global<TokenMetadata>(object::object_address(option::borrow(&body))).transfer_ref;
            option::swap_or_fill(&mut fighter_obj.body, new_body);
            creator_transfer(transfer_ref, fighter_address);
        };
        update_fighter_attributes(fighter);
    }

    // ======================================================================
    //   private helper functions //
    // ======================================================================

    fun mint_internal(
        admin: &signer,
        mint_to: address,
        collection: String,
        token_name: String,
        token_description: String,
        token_uri: String,
        type: String,
        health: u64,
        speed: u64,
    ) {
        let constructor_ref = token::create_from_account(
            admin,
            collection,
            token_description,
            token_name,
            option::none(),
            token_uri,
        );
        let object_signer = object::generate_signer(&constructor_ref);
        let transfer_ref = object::generate_transfer_ref(&constructor_ref);
        let mutator_ref = token::generate_mutator_ref(&constructor_ref);
        let burn_ref = token::generate_burn_ref(&constructor_ref);
        let property_mutator_ref = property_map::generate_mutator_ref(&constructor_ref);
        // Transfers the token to the `claimer` address
        let linear_transfer_ref = object::generate_linear_transfer_ref(&transfer_ref);
        object::transfer_with_ref(linear_transfer_ref, mint_to);
        // Add the traits to the object
        let attributes = Attributes {
            Speed: speed,
            Health: health,
            Type: type,
        };
        // Initialize the property map for display
        let properties = property_map::prepare_input(vector[], vector[], vector[]);
        property_map::init(&constructor_ref, properties);
        add_attributes_property_map(&property_mutator_ref, &attributes);
        // move attributes to the token
        move_to(&object_signer, attributes);
        // Move the object metadata to the token object
        let token_metadata = TokenMetadata {
            burn_ref,
            transfer_ref,
            mutator_ref,
            property_mutator_ref,
            mint_events: object::new_event_handle(&object_signer),
        };
        move_to(&object_signer, token_metadata);
        // Move the token specific struct to the token object
        if (type == utf8(FIGHTER_TYPE)) {
            let fighter = Fighter {
                body: option::none(),
                wings: option::none(),
            };
            // Also add base attributes to fighter
            let base_attributes = FighterBaseAttributes {
                Speed: speed,
                Health: health,
            };
            move_to(&object_signer, fighter);
            move_to(&object_signer, base_attributes);
        } else if (type == utf8(WING_TYPE)) {
            let wing = Wing {};
            move_to(&object_signer, wing);
        } else if (type == utf8(BODY_TYPE)) {
            let body = Body {};
            move_to(&object_signer, body);
        };
    }

    /// Add from all the parts and base attributes
    fun update_fighter_attributes(
        fighter: Object<Fighter>,
    ) acquires Fighter, FighterBaseAttributes, Attributes, TokenMetadata {
        let base_attributes = borrow_global<FighterBaseAttributes>(object::object_address(&fighter));
        let final_attributes = Attributes {
            Speed: base_attributes.Speed,
            Health: base_attributes.Health,
            Type: utf8(FIGHTER_TYPE),
        };
        let fighter_obj = borrow_global_mut<Fighter>(object::object_address(&fighter));
        let wings = fighter_obj.wings;
        let body = fighter_obj.body;
        if (option::is_some(&wings)) {
            let wing_attributes = borrow_global<Attributes>(object::object_address(option::borrow(&wings)));
            final_attributes.Speed = final_attributes.Speed + wing_attributes.Speed;
            final_attributes.Health = final_attributes.Health + wing_attributes.Health;
        };
        if (option::is_some(&body)) {
            let body_attributes = borrow_global<Attributes>(object::object_address(option::borrow(&body)));
            final_attributes.Speed = final_attributes.Speed + body_attributes.Speed;
            final_attributes.Health = final_attributes.Health + body_attributes.Health;
        };
        let attributes = borrow_global_mut<Attributes>(object::object_address(&fighter));
        attributes.Speed = final_attributes.Speed;
        attributes.Health = final_attributes.Health;
        // Finally, update the property map
        let property_mutator_ref = &borrow_global<TokenMetadata>(object::object_address(&fighter)).property_mutator_ref;
        update_attributes_property_map(property_mutator_ref, &final_attributes);
    }

    fun add_attributes_property_map(
        mutator_ref: &property_map::MutatorRef,
        attributes: &Attributes
    ) {
        property_map::add_typed(
            mutator_ref,
            utf8(b"Health"),
            attributes.Health,
        );
        property_map::add_typed(
            mutator_ref,
            utf8(b"Speed"),
            attributes.Speed,
        );
        property_map::add_typed(
            mutator_ref,
            utf8(b"Type"),
            attributes.Type,
        );
    }

    fun update_attributes_property_map(
        mutator_ref: &property_map::MutatorRef,
        attributes: &Attributes
    ) {
        property_map::update_typed(
            mutator_ref,
            &utf8(b"Health"),
            attributes.Health,
        );
        property_map::update_typed(
            mutator_ref,
            &utf8(b"Speed"),
            attributes.Speed,
        );
    }

    /// to can be user or object
    public(friend) fun creator_transfer(
        transfer_ref: &TransferRef,
        to: address,
    ) {
        let linear_transfer_ref = object::generate_linear_transfer_ref(transfer_ref);
        object::transfer_with_ref(linear_transfer_ref, to);
    }
}